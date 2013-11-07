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
        // TODO: The RFC says TEXTDATA are the characters: %x20-21 / %x23-2B / %x2D-7E
        // using Char.IsLetterOrDigit() is just a quick hack to prevent accepting *any*
        // character, so that the 'failwith' case can be hit.
        | c :: tail when Char.IsLetterOrDigit(c) -> innerTokenize (TEXTDATA(c) :: acc) tail
        | [] -> List.rev acc
        | c :: tail -> failwith (sprintf "unknown input '%c'!" c)

    // Turn the string into a list of characters so that we can lex it using
    // 'innerTokenize'...
    let chars = Array.toList (source.ToCharArray())

    // Tokenize the characters, seeding the accumulator 'acc' with an empty list.
    innerTokenize [] chars


[<EntryPoint>]
let main argv = 
    // Eventually (maybe) take a filename from the command-line.
    ////printfn "%A" argv

    // The source CSV we're attempting to parse...
    let source = "foo,bar\r\n\"simple\",\"with,comma\""

    printfn "Parsing source..."
    printfn "--------------------"
    printfn "%s" source
    printfn "--------------------"
    let tokens = tokenize source

    printfn "Tokens..."
    printfn "--------------------"
    List.iter (fun t -> printfn "  %O" t) tokens
    printfn "--------------------"

    // The ReadKey pauses the output window when we're running under VS's debugger.
    printfn ""
    printfn "Press any key to exit . . . "
    Console.ReadKey true |> ignore

    0 // return an integer exit code
