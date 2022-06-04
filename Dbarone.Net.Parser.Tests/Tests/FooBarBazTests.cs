namespace Dbarone.Net.Parser.Tests;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Xunit;

public class FooBarBazTests : AbstractTests
{
    [Theory]
    [InlineData("FOO", 1)]
    [InlineData("FOOBAR", 2)]
    [InlineData("FOOBARBAZ", 3)]
    [InlineData("FOOBARBAZBAZ", 4)]
    [InlineData("FOOBARBAZBAZBAZ", 5)]
    [InlineData("FOOBARBAR", 3)]
    [InlineData("FOOBARBARBAZ", 4)]
    [InlineData("FOOBARBARBARBAZ", 5)]
    public void TestFooBar(string input, object expected)
    {
        var grammar = FooBarBazGrammar;
        var rootProductionRule = "fbb";
        var visitor = FooBarBazVisitor;
        var resultMapper = ResultMapper;
        var actual = DoTest(grammar, input, rootProductionRule, visitor, resultMapper);
        Assert.Equal(expected, actual);
    }

    public string FooBarBazGrammar => @"
FOO     = ""FOO"";
BAR     = ""BAR"";
BAZ     = ""BAZ"";
fb      = :FOO,:BAR*;
fbb     = ITEMS:fb,ITEMS:BAZ*;
";

    private Visitor FooBarBazVisitor
    {
        get
        {
            // Initial state
            dynamic state = new ExpandoObject();
            state.items = new List<Token>();

            var visitor = new Visitor(state);

            visitor.AddVisitor(
                "fbb",
                (v, n) =>
                {
                    v.State.items = n.Properties["ITEMS"];
                }
            );

            return visitor;
        }
    }

    public static Func<dynamic, object> ResultMapper => (d) => d.items.Count;
}
