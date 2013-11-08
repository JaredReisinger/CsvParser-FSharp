module CsvParser.UnitTests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open CsvParser

[<TestClass>]
type LexerTests() = 

    // helper function to make writing the unit tests easier
    let testLexer source expected =
        let actual = tokenize source
        Assert.AreEqual(expected, actual)

    [<TestMethod>]
    member this.BasicTokensCanLex() =
        testLexer "x" (TEXTDATA('x') :: [])
        testLexer "," (COMMA :: [])
        testLexer "\r" (CR :: [])
        testLexer "\n" (LF :: [])
        testLexer "\"" (DQUOTE :: [])

    [<TestMethod>]
    member this.TwoFields() =
        testLexer "abc,def" (
            TEXTDATA('a') ::
            TEXTDATA('b') ::
            TEXTDATA('c') ::
            COMMA ::
            TEXTDATA('d') ::
            TEXTDATA('e') ::
            TEXTDATA('f') ::
            [])
