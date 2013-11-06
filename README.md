# CsvParser-FSharp

This project is a learning exercise, with the following goals:

 * Learn F#

 * Try to learn _idiomatic_ F#

 * Write an RFC 4180-compliant CSV (comma-separated values) parser (in F#, of
   course)

 * Learn how to structure unit tests for an F# project (the tests may or may
   not be written in F#, depending on how amenable VS is to that)

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

### TODO

Here's the plan of attack:

 * **DONE** <del>Stub out VS solution and project</del>

 * Lex input stream into tokens (note that this _can't_ be line-by-line
   because a quoted field can contain CR and LF)

 * Parse token stream into rows and fields

 * Create unit tests to cover CSV's edge cases.  (Ideally this could be done
   as or before the F# work happens, but will more likely be just after.)