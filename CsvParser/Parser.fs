module CsvParser.Parser

open System
open Lexer

// We don't *really* need to distinguish between unescaped and escaped fields, but
// doing so prevents us from losing information about the original source.
type token =
    | Unescaped of string
    | Escaped of string
    | NewLine

let format = function
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

    // Properly handling commas is tricky, as they can imply a field without
    // any intervening text, not only in the middle of a line, but also as the
    // first or last token (field).  Because of the recursive nature of our
    // parsing, it's not obvious when we see a comma if it's the same comma
    // that ended a previous field, or if it's a comma at the beginning of a
    // new line.  Rather than trying to track this "beginning of line" state
    // explicitly, we use two parsing helpers.  The "beginning of line" helper
    // *only* deals with a single leading comma, and delegates everything else
    // to the main helper.  The main helper can then assume that a comma
    // implies the *next* field, whether there's any content for it or not.
    let rec innerParseStart acc = function
        | COMMA :: tail -> innerParse (Unescaped("") :: acc) (COMMA :: tail)
        | anythingElse -> innerParse acc anythingElse

    // The "and" introduces a mutually recursive function, so that they
    // can call each other.
    and innerParse acc = function
        | COMMA :: DQUOTE :: tail       // start of escaped field
        | DQUOTE :: tail ->             // start of escaped field
            let s, rem = getEscapedText "" tail
            innerParse (Escaped(s) :: acc) rem
        | COMMA :: TEXTDATA(c) :: tail  // start of unescaped field
        | TEXTDATA(c) :: tail ->        // start of unescaped field
            let s, rem = getUnescapedText (c.ToString()) tail
            innerParse (Unescaped(s) :: acc) rem
        | COMMA :: tail -> innerParse (Unescaped("") :: acc) tail
        | CR :: LF :: tail -> innerParseStart (NewLine :: acc) tail
        | [] -> List.rev acc
        | token :: tail -> failwith (sprintf "Unexpected token '%s'" (Lexer.format token))

    // Start the parsing!
    innerParseStart [] tokens
