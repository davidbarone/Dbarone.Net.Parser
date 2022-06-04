namespace Dbarone.Net.Parser.Tests;
using System;
using System.Collections.Generic;
using System.Dynamic;
using Xunit;

public class SubRuleTests : AbstractTests
{
    [Theory]
    [InlineData("4", 4)]
    [InlineData("-4", -4)]
    [InlineData("9+9", 18)]
    [InlineData("1+2+3+4", 10)]
    [InlineData("2*3", 6)]
    [InlineData("1+2*3", 7)]
    [InlineData("(1+2)*3", 9)]
    [InlineData("2*-3", -6)]
    [InlineData("-2*-3", 6)]
    [InlineData("3*4+5*6", 42)]
    [InlineData("7-4", 3)]
    [InlineData("10-3+2", 9)]
    [InlineData("10-2*3+4*5", 24)]
    [InlineData("10--2*3+4*5", 36)]
    [InlineData("10+8/2-2*5", 4)]
    [InlineData("((((1+7)/(3-1))/2)*(5+2)+(-7+15)-(-2*-4))", 14)]
    [InlineData("6*2/3", 4)]
    public void TestSubRule(string input, object expected)
    {
        var grammar = SubRuleGrammar;
        var rootProductionRule = "expression";
        var visitor = SubRuleVisitor;
        var resultMapper = ResultMapper;
        var actual = DoTest(grammar, input, rootProductionRule, visitor, resultMapper);
        Assert.Equal(expected, actual);
    }

    public string SubRuleGrammar => @"
NUMBER_LITERAL  = ""\d+"";
PLUS_OP         = ""\+"";
MINUS_OP        = ""\-"";
MUL_OP          = ""\*"";
DIV_OP          = ""\/"";
LPAREN         = ""\("";
RPAREN         = ""\)"";

expression      = TERMS:(:term, :(OP:MINUS_OP, term | OP:PLUS_OP, term)*);
term            = FACTORS:(:factor, :(OP:DIV_OP, factor | OP:MUL_OP, factor)*);
factor          = primary | PLUS_OP, primary | MINUS_OP, primary;
primary         = NUMBER_LITERAL | LPAREN!, expression, RPAREN!;";

    public Visitor SubRuleVisitor
    {
        get
        {
            // Initial state
            dynamic state = new ExpandoObject();
            state.Stack = new Stack<int>();

            var visitor = new Visitor(state);

            visitor.AddVisitor(
                "expression",
                (v, n) =>
                {
                    int sum = 0;
                    var nodes = (IEnumerable<Object>)n.Properties["TERMS"];
                    foreach (var item in nodes)
                    {
                        var node = ((Node)item);
                        node.Accept(v);

                        if (!node.Properties.ContainsKey("OP"))
                        {
                            sum = (int)v.State.Stack.Pop();
                        }
                        else
                        {
                            var sign = ((Token)node.Properties["OP"]).TokenValue;
                            if (sign == "+")
                            {
                                sum = sum + (int)v.State.Stack.Pop();
                            }
                            else
                            {
                                sum = sum - (int)v.State.Stack.Pop();
                            }
                        }
                    }
                    v.State.Stack.Push(sum);
                }
            );

            visitor.AddVisitor(
                "term",
                (v, n) =>
                {
                    int sum = 0;
                    var nodes = (IEnumerable<Object>)n.Properties["FACTORS"];
                    foreach (var item in nodes)
                    {
                        var node = ((Node)item);
                        node.Accept(v);

                        if (!node.Properties.ContainsKey("OP"))
                        {
                            sum = (int)v.State.Stack.Pop();
                        }
                        else
                        {
                            var sign = ((Token)node.Properties["OP"]).TokenValue;
                            if (sign == "*")
                            {
                                sum = sum * (int)v.State.Stack.Pop();
                            }
                            else
                            {
                                sum = sum / (int)v.State.Stack.Pop();
                            }
                        }
                    }
                    v.State.Stack.Push(sum);
                }
            );

            visitor.AddVisitor(
                "factor",
                (v, n) =>
                {
                    var node = (Node)n.Properties["primary"];
                    node.Accept(v);
                    var hasMinus = n.Properties.ContainsKey("MINUS_OP");
                    int number = v.State.Stack.Pop();
                    if (hasMinus)
                        number = number * -1;
                    v.State.Stack.Push(number);
                }
            );

            visitor.AddVisitor(
                "primary",
                (v, n) =>
                {
                    if (n.Properties.ContainsKey("NUMBER_LITERAL"))
                    {
                        var number = int.Parse(((Token)n.Properties["NUMBER_LITERAL"]).TokenValue);
                        v.State.Stack.Push(number);
                    }
                    else
                    {
                        var expr = (Node)n.Properties["expression"];
                        expr.Accept(v);
                        int result = (int)v.State.Stack.Pop();
                        v.State.Stack.Push(result);
                    }
                }
            );

            visitor.AddVisitor(
                "mul_term",
                (v, n) =>
                {
                    bool one = false;
                    int sum = 0;
                    var nodes = (IEnumerable<Object>)n.Properties["FACTORS"];
                    foreach (var node in nodes)
                    {
                        ((Node)node).Accept(v);
                        if (!one)
                        {
                            sum = (int)v.State.Stack.Pop();
                            one = true;
                        }
                        else
                            sum = (int)v.State.Stack.Pop() * sum;
                    }
                    v.State.Stack.Push(sum);
                }
            );

            visitor.AddVisitor(
                "div_term",
                (v, n) =>
                {

                    var hasMinus = n.Properties.ContainsKey("MINUS_OP");
                    int number = v.State.Stack.Pop();
                    if (hasMinus)
                        number = number * -1;
                    v.State.Stack.Push(number);
                }
            );

            return visitor;
        }
    }

    public static Func<dynamic, object> ResultMapper => (d) => (int)d.Stack.Pop();
}
