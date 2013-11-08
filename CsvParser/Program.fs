module CsvParser.Program

open System
open Lexer
open Parser

// See http://www.ietf.org/rfc/rfc4180.txt for the RFC on the CSV format

let printListItems formatter list =
    List.iter (fun item -> formatter item |> printfn "  %s") list

[<EntryPoint>]
let main argv = 
    // Eventually (maybe) take a filename from the command-line.
    ////printfn "%A" argv

    // The source CSV we're attempting to parse...
    let source = "foo,bar\r\n\"simple\",\"with,comma\"\r\n\"with\rCR\nLF\",\"with\"\"quote\""

    printfn "Source..."
    printfn "--------------------"
    printfn "%s" source
    printfn "--------------------"

    printfn "Lexer Tokens..."
    printfn "--------------------"
    let lexerTokens = lex source
    printListItems Lexer.format lexerTokens
    printfn "--------------------"

    printfn "Parser Tokens..."
    printfn "--------------------"
    let parserTokens = parse lexerTokens
    printListItems Parser.format parserTokens
    printfn "--------------------"

    // The ReadKey pauses the output window when we're running under VS's debugger.
    printfn ""
    printfn "Press any key to exit . . . "
    Console.ReadKey true |> ignore

    0 // return an integer exit code
