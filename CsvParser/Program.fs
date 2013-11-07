open System

// See http://www.ietf.org/rfc/rfc4180.txt for the RFC on the CSV format

// The names of the lexer tokens come straight from the RFC's ABNF grammar
// for the CSV format. 
type lexerToken =
    | COMMA
    | CR
    | LF
    | DQUOTE
    | TEXTDATA of char

// Unlike Enums which have an automatic ToString() return of their value,
// discriminated unions in F# (which is what 'lexerTokens' is) do not
// seem to.  We need to create a helper function if we want to display
// token values for diagnostic purposes.
let formatToken = function
    | COMMA -> "COMMA"
    | CR -> "CR"
    | LF -> "LF"
    | DQUOTE -> "DQUOTE"
    | TEXTDATA(c) -> sprintf "TEXTDATA(%c)" c

// tokenize: lexes a string into individual tokens.  Note that this is
// a precursor to parsing, where quoting/escaping is handled.  The
// tokenizer *only* maps characters to their lexerToken value, it does
// not attempt to understand the semantics of the tokens.
let tokenize (source : string) =
    // We hide the recursive function inside of 'tokenize', so that callers
    // don't have to think about how the string is transformed to a list of
    // lexerTokens, nor be aware that it's via a (tail-)recursive function
    // with an accumulator.
    //
    // Idiomatic F# creates lists by *prepending* new values, because that's
    // an O(1) operation on the linked-list.  Because of this, the list has
    // to be reversed (see the '[]' case) once everything has been accumulated.
    let rec innerTokenize acc = function
        | ',' :: tail -> innerTokenize (COMMA :: acc) tail
        | '\r' :: tail -> innerTokenize (CR :: acc) tail
        | '\n' :: tail -> innerTokenize (LF :: acc) tail
        | '\"' :: tail -> innerTokenize (DQUOTE :: acc) tail
        // The RFC says TEXTDATA are the characters in the range 0x20-0x7E, except
        // for 0x22 (DQUOTE) and 0x2C (COMMA).  This works out to ' ' <= c <= '~'
        | c :: tail when ' ' <= c && c <= '~' -> innerTokenize (TEXTDATA(c) :: acc) tail
        | [] -> List.rev acc
        | c :: tail -> failwith (sprintf "Invalid input character '%c' (0x%02x).  Only ASCII is supported." c (int c))

    // Turn the string into a list of characters so that we can lex it using
    // 'innerTokenize'...
    // Note: The RFC says that "Common usage of CSV is US-ASCII" but allows for
    // other character sets.  Yet the definition of TEXTDATA restricts values
    // into a 7-bit range.  We're going to simply ignore the issue of larger
    // Unicode code points and assume that each char in the string stands for
    // an entire character.
    let chars = Array.toList (source.ToCharArray())

    // Tokenize the characters, seeding the accumulator 'acc' with an empty list.
    innerTokenize [] chars

type parserToken =
    | Unescaped of string
    | Escaped of string
    | NewLine

let formatParserToken = function
    | Unescaped(s) -> sprintf "Unescaped(%s)" s
    | Escaped(s) -> sprintf "Escaped(%s)" s
    | NewLine -> "NewLine"

let parse tokens =
    // Just like tokenization, we use inner recursive functions to do the
    // heavy-lifting parts of parsing.  Parsing is more complicated, though
    // as we pass off to additional helpers to handle accumulating the
    // unescaped and escaped fields.  Note that these helpers return a
    // *tuple* of the string value and the remaining lexer tokens so that
    // the calling function can easily pick up where the helper function
    // stops.
    let rec getUnescapedText acc = function
        | TEXTDATA(c) :: tail -> getUnescapedText (acc + c.ToString()) tail
        | [] -> acc, []
        | tail -> acc, tail // anything else terminates

    let rec getEscapedText acc = function
        | TEXTDATA(c) :: tail -> getEscapedText (acc + c.ToString()) tail
        | CR :: tail -> getEscapedText (acc + "\r") tail
        | LF :: tail -> getEscapedText (acc + "\n") tail
        | COMMA :: tail -> getEscapedText (acc + ",") tail
        | DQUOTE :: DQUOTE :: tail -> getEscapedText (acc + "\"") tail
        | DQUOTE :: tail -> acc, tail // a single double-quote terminates the escaped text
        | [] -> acc, []

    // Properly handlng commas is tricky, as they can *imply* a field
    // without any intervening text.  We handle this by explicitly looking
    // at the token after the COMMA, and lumping it together with the non-
    // comma case (these are the nth and first field patterns, respectively).
    // If we haven't matched yet, but see we have a comma (*without* a
    // following DQUOTE or TEXTDATA) it's the COMMA preceeding an empty
    // field.  This works even for the last field on a line.
    let rec innerParse acc = function
        | COMMA :: DQUOTE :: tail       // start of escaped field
        | DQUOTE :: tail ->             // start of escaped field
            let s, rem = getEscapedText "" tail
            innerParse (Escaped(s) :: acc) rem
        | COMMA :: TEXTDATA(c) :: tail  // start of unescaped field
        | TEXTDATA(c) :: tail ->        // start of unescaped field
            let s, rem = getUnescapedText (c.ToString()) tail
            innerParse (Unescaped(s) :: acc) rem
        | COMMA :: tail -> innerParse (Unescaped("") :: acc) tail
        | CR :: LF :: tail -> innerParse (NewLine :: acc) tail
        | [] -> List.rev acc
        | token :: tail -> failwith (sprintf "Unexpected token '%s'" (formatToken token))

    // Start the parsing!
    innerParse [] tokens

let printListItems formatter list =
    List.iter (fun item -> formatter item |> printfn "  %s") list

[<EntryPoint>]
let main argv = 
    // Eventually (maybe) take a filename from the command-line.
    ////printfn "%A" argv

    // The source CSV we're attempting to parse...
    ////let source = "foo,bar\r\n\"simple\",\"with,comma\""
    ////let source = "foo,bar"
    let source = "foo,bar\r\n\"simple\",\"with,comma\"\r\n\"with\rCR\nLF\",\"with\"\"quote\""
    // Some invalid characters, just to test...
    ////let source = "abc\bdef"

    printfn "Source..."
    printfn "--------------------"
    printfn "%s" source
    printfn "--------------------"

    printfn "Lexer Tokens..."
    printfn "--------------------"
    let tokens = tokenize source
    printListItems formatToken tokens
    printfn "--------------------"

    printfn "Parser Tokens..."
    printfn "--------------------"
    let parserTokens = parse tokens
    printListItems formatParserToken parserTokens
    printfn "--------------------"

    // The ReadKey pauses the output window when we're running under VS's debugger.
    printfn ""
    printfn "Press any key to exit . . . "
    Console.ReadKey true |> ignore

    0 // return an integer exit code
