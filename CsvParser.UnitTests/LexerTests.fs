namespace CsvParser.UnitTests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open CsvParser.Lexer

[<TestClass>]
type LexerTests() = 

    // helper function to make writing the unit tests easier
    let testLexer source expected =
        let actual = lex source
        Assert.AreEqual(expected, actual)

    [<TestMethod>]
    member this.Lexer_BasicTokensCanLex() =
        testLexer "x" (TEXTDATA('x') :: [])
        testLexer "," (COMMA :: [])
        testLexer "\r" (CR :: [])
        testLexer "\n" (LF :: [])
        testLexer "\"" (DQUOTE :: [])

    [<TestMethod>]
    member this.Lexer_TwoFields() =
        testLexer "abc,def" (
            TEXTDATA('a') ::
            TEXTDATA('b') ::
            TEXTDATA('c') ::
            COMMA ::
            TEXTDATA('d') ::
            TEXTDATA('e') ::
            TEXTDATA('f') ::
            [])

    [<TestMethod>]
    [<ExpectedException(typeof<System.Exception>)>]
    member this.Lexer_UnknownCharactersWillThrow() =
        testLexer "a\bc" []

    // Since the lexer doesn't attempt to have any deep understanding about
    // the tokens (it neither knows nor cares that "a quoted string means
    // that the field may contain commas, quotes, CRs, or LFs"), there aren't
    // a lot of tests that we need here.  The parser, on the other hand, will
    // be much more involved.
