namespace CsvParser.UnitTests

open System
open Microsoft.VisualStudio.TestTools.UnitTesting
open CsvParser.Lexer
open CsvParser.Parser

[<TestClass>]
type ParserTests() = 

    // helper function to make writing the unit tests easier
    let testParser lexerTokens expected =
        let actual = parse lexerTokens
        Assert.AreEqual(expected, actual)

    [<TestMethod>]
    member this.Parser_SingleFieldOneChar() =
        testParser
            (TEXTDATA('x') :: [])
            (Unescaped("x") :: [])

    [<TestMethod>]
    member this.Parser_SingleFieldMultiChar() =
        testParser
            (TEXTDATA('a') :: TEXTDATA('b') :: TEXTDATA('c') :: [])
            (Unescaped("abc") :: [])

    [<TestMethod>]
    member this.Parser_TwoFields() =
        testParser
            (TEXTDATA('a') :: TEXTDATA('b') :: TEXTDATA('c') ::
            COMMA ::
            TEXTDATA('d') :: TEXTDATA('e') :: TEXTDATA('f') :: [])

            (Unescaped("abc") :: Unescaped("def") :: [])

    [<TestMethod>]
    member this.Parser_TrailingCommaImpliesEmptyField() =
        testParser
            (TEXTDATA('a') :: TEXTDATA('b') :: TEXTDATA('c') :: COMMA :: [])
            (Unescaped("abc") :: Unescaped("") :: [])

    [<TestMethod>]
    member this.Parser_LeadingCommaImpliesEmptyField() =
        testParser
            (COMMA :: TEXTDATA('a') :: TEXTDATA('b') :: TEXTDATA('c') :: [])
            (Unescaped("") :: Unescaped("abc") :: [])

    [<TestMethod>]
    member this.Parser_SingleCommaShouldBeTwoFields() =
        testParser
            (COMMA :: [])
            (Unescaped("") :: Unescaped("") :: [])

    [<TestMethod>]
    member this.Parser_TwoLines() =
        testParser

            (TEXTDATA('a') :: TEXTDATA('b') :: TEXTDATA('c') ::
            CR :: LF ::
            TEXTDATA('d') :: TEXTDATA('e') :: TEXTDATA('f') :: [])

            (Unescaped("abc") :: NewLine :: Unescaped("def") :: [])

    [<TestMethod>]
    [<ExpectedException(typeof<System.Exception>)>]
    member this.Parser_TwoLinesWithOnlyCRThrows() =
        testParser

            (TEXTDATA('a') :: TEXTDATA('b') :: TEXTDATA('c') ::
            CR ::
            TEXTDATA('d') :: TEXTDATA('e') :: TEXTDATA('f') :: [])
            // Note that we don't actually get the result...
            (Unescaped("abc") :: NewLine :: Unescaped("def") :: [])

    [<TestMethod>]
    [<ExpectedException(typeof<System.Exception>)>]
    member this.Parser_TwoLinesWithOnlyLFThrows() =
        testParser

            (TEXTDATA('a') :: TEXTDATA('b') :: TEXTDATA('c') ::
            LF ::
            TEXTDATA('d') :: TEXTDATA('e') :: TEXTDATA('f') :: [])
            // Note that we don't actually get the result...
            (Unescaped("abc") :: NewLine :: Unescaped("def") :: [])

    [<TestMethod>]
    member this.Parser_TwoFieldsTwoLines() =
        testParser

            (TEXTDATA('a') :: TEXTDATA('b') :: TEXTDATA('c') ::
            COMMA ::
            TEXTDATA('d') :: TEXTDATA('e') :: TEXTDATA('f') :: 
            CR :: LF ::
            TEXTDATA('g') :: TEXTDATA('h') :: TEXTDATA('i') ::
            COMMA ::
            TEXTDATA('j') :: TEXTDATA('k') :: TEXTDATA('l') :: [])

            (Unescaped("abc") :: Unescaped("def") :: NewLine ::
            Unescaped("ghi") :: Unescaped("jkl") :: [])

    [<TestMethod>]
    member this.Parser_EscapedSingleFieldOneChar() =
        testParser
            (DQUOTE :: TEXTDATA('x') :: DQUOTE :: [])
            (Escaped("x") :: [])

    [<TestMethod>]
    member this.Parser_EscapedFieldWithComma() =
        testParser
            (DQUOTE :: TEXTDATA('x') :: COMMA :: TEXTDATA('y') :: DQUOTE :: [])
            (Escaped("x,y") :: [])

    [<TestMethod>]
    member this.Parser_EscapedFieldWithQuote() =
        testParser
            (DQUOTE :: TEXTDATA('x') :: DQUOTE :: DQUOTE :: TEXTDATA('y') :: DQUOTE :: [])
            (Escaped("x\"y") :: [])

    [<TestMethod>]
    member this.Parser_EscapedFieldWithCR() =
        testParser
            (DQUOTE :: TEXTDATA('x') :: CR :: TEXTDATA('y') :: DQUOTE :: [])
            (Escaped("x\ry") :: [])

    [<TestMethod>]
    member this.Parser_EscapedFieldWithLF() =
        testParser
            (DQUOTE :: TEXTDATA('x') :: LF :: TEXTDATA('y') :: DQUOTE :: [])
            (Escaped("x\ny") :: [])

    // This next test has input that looks like (in C# syntax):
    //    "field1,field2,field3\r\n" +
    //    "\"aaa\r\n\",\"bb,b\",\"ccc\"\r\n" + 
    //    "\"in \"\"quotes\"\"\",2,3\r\n" + 
    //    "1,2,\r\n" + 
    //    "zzz,yyy,xxx\r\n" + 
    //    "1,,3\r\n" + 
    //    ",,";

    [<TestMethod>]
    member this.Parser_TestEverything() =
        testParser
            (
            TEXTDATA('f') :: TEXTDATA('i') :: TEXTDATA('e') :: TEXTDATA('l') :: TEXTDATA('d') :: TEXTDATA('1') ::
            COMMA ::
            TEXTDATA('f') :: TEXTDATA('i') :: TEXTDATA('e') :: TEXTDATA('l') :: TEXTDATA('d') :: TEXTDATA('2') ::
            COMMA ::
            TEXTDATA('f') :: TEXTDATA('i') :: TEXTDATA('e') :: TEXTDATA('l') :: TEXTDATA('d') :: TEXTDATA('3') ::
            CR :: LF ::
            DQUOTE :: TEXTDATA('a') :: TEXTDATA('a') :: TEXTDATA('a') :: CR :: LF :: DQUOTE ::
            COMMA ::
            DQUOTE :: TEXTDATA('b') :: TEXTDATA('b') :: COMMA :: TEXTDATA('b') :: DQUOTE ::
            COMMA ::
            DQUOTE :: TEXTDATA('c') :: TEXTDATA('c') :: TEXTDATA('c') :: DQUOTE ::
            CR :: LF ::
            DQUOTE :: TEXTDATA('i') :: TEXTDATA('n') :: TEXTDATA(' ') :: DQUOTE :: DQUOTE :: TEXTDATA('q') :: TEXTDATA('u') :: TEXTDATA('o') :: TEXTDATA('t') :: TEXTDATA('e') :: TEXTDATA('s') :: DQUOTE :: DQUOTE :: DQUOTE ::
            COMMA ::
            TEXTDATA('2') ::
            COMMA ::
            TEXTDATA('3') ::
            CR :: LF ::
            TEXTDATA('1') ::
            COMMA ::
            TEXTDATA('2') ::
            COMMA ::
            CR :: LF ::
            TEXTDATA('z') :: TEXTDATA('z') :: TEXTDATA('z') ::
            COMMA ::
            TEXTDATA('y') :: TEXTDATA('y') :: TEXTDATA('y') ::
            COMMA ::
            TEXTDATA('x') :: TEXTDATA('x') :: TEXTDATA('x') ::
            CR :: LF ::
            TEXTDATA('1') ::
            COMMA ::
            COMMA ::
            TEXTDATA('3') ::
            CR :: LF ::
            COMMA ::
            COMMA ::
            []
            )

            (
            Unescaped("field1") :: Unescaped("field2") :: Unescaped("field3") :: NewLine ::
            Escaped("aaa\r\n") :: Escaped("bb,b") :: Escaped("ccc") :: NewLine ::
            Escaped("in \"quotes\"") :: Unescaped("2") :: Unescaped("3") :: NewLine ::
            Unescaped("1") :: Unescaped("2") :: Unescaped("") :: NewLine ::
            Unescaped("zzz") :: Unescaped("yyy") :: Unescaped("xxx") :: NewLine ::
            Unescaped("1") :: Unescaped("") :: Unescaped("3") :: NewLine ::
            Unescaped("") :: Unescaped("") :: Unescaped("") ::
            []
            )

