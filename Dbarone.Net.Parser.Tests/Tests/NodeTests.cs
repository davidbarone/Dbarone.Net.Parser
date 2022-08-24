namespace Dbarone.Net.Parser.Tests;
using System;
using System.Collections.Generic;
using System.Dynamic;
using Xunit;

public class NodeTests : AbstractTests
{
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

    [Fact]
    public void PrettyPrintTest()
    {

        var expected = @"+- expression
   +- term
   |  +- factor
   |     +- primary
   |        +- ""NUMBER_LITERAL"" NUMBER_LITERAL [9]
   +- anonymous_1
      +- ""OP"" PLUS_OP [+]
      +- term
         +- factor
            +- primary
               +- ""NUMBER_LITERAL"" NUMBER_LITERAL [5]
";

        var input = "9+5";
        var parser = new Parser(SubRuleGrammar, "expression");
        var ast = parser.Parse(input);
        var actual = ast.PrettyPrint();

        Assert.Equal(expected, actual);
    }
}
