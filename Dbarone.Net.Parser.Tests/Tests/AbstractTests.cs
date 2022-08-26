namespace Dbarone.Net.Parser.Tests;
using System;

public abstract class AbstractTests
{
    /// <summary>
    /// Executes a single test.
    /// </summary>
    /// <remarks>
    /// If no input set, then only grammar is checked for correctness.
    /// If input + productionRule set, then parsing occurs.
    /// If visitor set, then ast is traversed, and a result is calculated.
    /// </remarks>
    /// <param name="grammar">The grammar used for the test.</param>
    /// <param name="input">The input to parse.</param>
    /// <param name="productionRule">The root production rule to use.</param>
    /// <param name="visitor">Visitor to use to navigate ast.</param>
    /// <param name="resultMapping">Mapping of result.</param>
    /// <param name="expected">Expected result.</param>
    /// <param name="expectException">The expected result.</param>
    protected object? DoTest<TState>(string grammar, string input, string rootProductionRule, Visitor<TState> visitor, Func<dynamic, object> resultMapper)
    {
        var parser = new Parser(grammar, rootProductionRule);

        if (!string.IsNullOrEmpty(input))
        {
            var ast = parser.Parse(input);
            if (visitor != null)
            {
                ast.Walk(visitor);
                var actual = resultMapper(visitor.State!);
                return actual;
            }
        }
        return null;
    }
}
