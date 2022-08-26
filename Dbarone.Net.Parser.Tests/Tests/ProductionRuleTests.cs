namespace Dbarone.Net.Parser.Tests;
using System;
using Xunit;

/// <summary>
/// Tests that grammars can be parsed. NOTE the parser
/// does not check the actual content validity of the grammar - 
/// It only checks that the grammar is syntactically correct.
/// </summary>
public class ProductionRuleTests : AbstractTests
{

    [Theory]
    [InlineData(@"SIMPLE = ""X"";")]
    [InlineData(@"SIMPLE = ""X""; ANOTHER = ""Y"";")]
    [InlineData(@"
/* This is a test */
SIMPLE  = ""X"";
ANOTHER = ""Y"";")]
    [InlineData(@"
SIMPLE  = ""X""; /* This is a comment */
/* Another comment */
ANOTHER = ""Y"";")]
    [InlineData(@"
SIMPLE  = ""X"";
ANOTHER = ""Y"";
rule    = SIMPLE;")]
    [InlineData(@"myrule = SIMPLE,ANOTHER;")] // Note this grammer is incomplete - however, syntactically, it is correct.
    [InlineData(@"myrule = TEST:SIMPLE*;")]
    [InlineData(@"myrule = SIMPLE, ANOTHER | SIMPLE;")]
    public void TestValidGrammar(string? grammar)
    {
        // Should not throw exception
        DoTest<dynamic>(grammar!, null!, null!, null!, null!);
    }

    [Theory]
    [InlineData(null, "Invalid grammar. No production rules found.")]
    [InlineData("", "Invalid grammar. No production rules found.")]
    [InlineData(" ", "Syntax error near")]
    [InlineData("(* JUST A COMMENT *)", "Syntax error near")]
    [InlineData("rule1 ", "Syntax error near")]
    [InlineData("rule1", "Input cannot be parsed")] // This one may need to be checked. Looks like no space at end results in different error
    [InlineData(@"test = ""ABC", "Syntax error near")]
    public void TestInvalidGrammar(string? grammar, string expectedError)
    {
        var ex = Assert.Throws<Exception>(() => DoTest<dynamic>(grammar!, null!, null!, null!, null!));
        Assert.StartsWith(expectedError, ex.Message);
    }
}