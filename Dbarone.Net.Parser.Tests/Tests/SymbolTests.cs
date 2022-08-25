namespace Dbarone.Net.Parser.Tests;
using System;
using System.Collections.Generic;
using System.Dynamic;
using Xunit;

public class SymbolTests
{
    MatchResult SymbolMatch(string pattern, string input)
    {
        var symbol = new Symbol(pattern, RuleType.LexerRule);
        return symbol.Match(input);
    }

    [Theory]
    [InlineData("[:]", ": There is a colon at the start here.", true, ":", " There is a colon at the start here.")]
    [InlineData(@"""(?:[^""]|\\.)*""", @"""This is some quoted text""; Rest of file;", true, @"""This is some quoted text""", "; Rest of file;")]
    [InlineData("[(]", "This input has no left bracket", false, null, null)]
    [InlineData("[(]", "This input has a left ( bracket but not at start", false, null, null)]
    [InlineData("[(]", " ( This input has a left bracket at the start, prepended by whitespace", true, "(", " This input has a left bracket at the start, prepended by whitespace")]
    [InlineData("[(]", "( This input has a left bracket at the start", true, "(", " This input has a left bracket at the start")]
    [InlineData("[(]", @"
( This input has left bracket at the start, but is on second line.
The input is multiple lines.", true, "(", @" This input has left bracket at the start, but is on second line.
The input is multiple lines.")]
    [InlineData("[(]", @"
a( This input has left bracket NOT at the start, and is on second line.
The input is multiple lines.", false, null, null)]
    public void TestSymbolMatching(string pattern, string input, bool expectedSuccess, string? expectedMatch, string? expectedRemainder)
    {
        var result = SymbolMatch(pattern, input);
        Assert.Equal(expectedSuccess, result.Success);
        if (expectedSuccess)
        {
            Assert.Equal(expectedMatch, result.Matched);
            Assert.Equal(expectedRemainder, result.Remainder);
        }
    }
}
