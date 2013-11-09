module CsvParser.Restructurer

open System
open Parser

// There is really no reason why the Parser shouldn't just return a list of
// list of strings (or a string list list in F# parlance).  I'm leaving the
// code as-is, though as there is some diagnostic value in seeing the
// unescaped vs. escaped fields, and conflating the lexerToken-to-field
// parsing and the linear-list-to-nested-list manipulation makes things less
// obvious.

let restructure tokens =

    let rec restructureLine acc = function
        | Unescaped(x) :: tail
        | Escaped (x) :: tail ->
            restructureLine (x :: acc) tail
        | NewLine :: tail ->
            List.rev acc, tail
        | [] -> List.rev acc, []

    let rec restructureInner acc = function
        | Unescaped(x) :: tail
        | Escaped (x) :: tail ->
            let line, rem = restructureLine (x :: []) tail
            restructureInner (line :: acc) rem
        | NewLine :: tail -> restructureInner ([] :: acc) tail
        | [] -> List.rev acc

    restructureInner [] tokens
