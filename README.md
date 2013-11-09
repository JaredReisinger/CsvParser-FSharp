# CsvParser-FSharp

This project is a learning exercise, with the following goals:

 * Learn F#

 * Try to learn _idiomatic_ F#

 * Write an RFC 4180-compliant CSV (comma-separated values) parser (in F#, of
   course)

 * Learn how to structure unit tests for an F# project <del>(the tests may
   or may not be written in F#, depending on how amenable VS is to that)</del>
   _It turns out that writing the unit tests in F# isn't hard at all!  See
   the [F# and Unit testing](#FSharpUnitTesting) section to find out more._

Coming from a C/C++/C# background, learning to think in a "functional" way
takes time and practice.  There are a bunch of resources out there that try
to explain F#, but many of them either assume you already think in functional
terms, or just attempt to show how to do C++/C# kinds of things in F#.  For a
good from-the-ground-up explanation, the book
[F# Programming](http://en.wikibooks.org/wiki/F_Sharp_Programming)
on WikiBooks seems really solid.  (At the very least, it's structured in the
way that I like to learn.  YMMV)

### Caveats

As part and parcel of this being a learning exercise, certain design choices
have been made that _are not_ what one would likely do for a "real" parser:

 * The parser will be a standalone EXE rather than a library; if you want
   to use the code in a library feel free to do so.

 * Rather than using the FsLex and FsYacc tools that come with F#, the lexer
   and tokenizer will be hand-written, and regular expressions avoided where
   possible.

 * There may be an obnoxious number of comments in the source files, at least
   from the perspective of an experienced F# developer.  These are meant as
   reminders of lessons learned, and for the benefit (hopefully!) of any
   C/C++/C#-esque developers.

 * All of the work is being done directly in the master branch, even if that
   means the master branch isn't super stable.

 * There will probably be other shortcomings I can't anticipate; they will be
   documented here when they are discovered.  (If you're an experienced F#
   developer and see something amiss, please feel free to let me know, and
   I'll either fix it or add it to this list!)

### TO-DO

Here's the plan of attack:

 * **DONE** <del>Stub out VS solution and project.</del>

 * **DONE** <del>Lex input stream into tokens (note that this _can't_ be
   line-by-line because a quoted field can contain `CR` and `LF`).</del>

 * **DONE** <del>Find a way to see the values in the lexerToken discriminated
   union; they don't seem to pretty-print the way that C# Enums do.</del>

 * **DONE** <del>Parse token stream into rows and fields.
   <em>The parser currently creates "unescaped fields" and "escaped (quoted)
   fields", both of which have a string value, and a "newline" marker.
   These have not yet been flattened into a simple `string[][]`</em></del>

 * **DONE** <del>Create unit tests to cover CSV's edge cases.  (Ideally
   this could be done as or before the F# work happens, but will more likely
   be just after.)</del>

--------------------------------------------------

## Other Notes

### <span id="FSharpUnitTesting">F# and Unit testing<span>

A lot of the online references that can be found for
writing unit tests in F# seem to be out-of-date.  In Visual Studio 2012 (v11),
you can simply create an F# library, add a reference to
"Microsoft.VisualStudio.QualityTools.UnitTestFramework" (and to the assembly
you want to test), and start writing tests.  The built-in test handlers
seem to detect and run the tests just fine (using 'Test'->'Run'->... from the
menu).
