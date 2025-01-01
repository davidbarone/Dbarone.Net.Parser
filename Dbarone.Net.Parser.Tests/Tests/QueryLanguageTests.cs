namespace Dbarone.Net.Parser.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Dbarone.Net.Extensions.Reflection;


public class QueryLanguageState
{
    public Stack<int> Stack = new Stack<int>();
    public Stack<Func<Customer, bool>> FilterFunctions = new Stack<Func<Customer, bool>>();
}

/// <summary>
/// This test suite illustrates a very simple query language called Sqlish.
/// This shows how a more complex grammar can be used to query & filter
/// a list of objects.
/// </summary>
public class QueryLanguageTests : AbstractTests
{

    /// <summary>
    /// Defines the grammar of QueryLanguage.
    /// </summary>
    public string QueryLanguageGrammar => @"

/* Lexer rules */

/* keywords */
DATA_TYPE_INTEGER = ""\bINT\b"";
DATA_TYPE_REAL = ""\bREAL\b"";
DATA_TYPE_TEXT = ""\bTEXT\b"";
DATA_TYPE_DATETIME = ""\bDATETIME\b"";
NULL = ""\bNULL\b"";
NOT = ""\bNOT\b"";


CREATE = ""\bCREATE\b"";
COLLECTION = ""\bCOLLECTION\b"";
LEFT_PAREN = ""[(]"";
RIGHT_PAREN = ""[)]"";
COMMA = "","";
IDENTIFIER      = ""[A-Z_][A-Z_0-9]*"";

/* Parser rules */

column_name = :IDENTIFIER;
not_null = :(:NOT, :NULL);
data_type = :DATA_TYPE_INTEGER | :DATA_TYPE_REAL | :DATA_TYPE_TEXT | :DATA_TYPE_DATETIME;
column_definition = COLUMN_NAME:column_name, DATA_TYPE:data_type, NOT_NULL:not_null?;
collection_element_list = LEFT_PAREN!, :column_definition, :(COMMA!, :column_definition)*, RIGHT_PAREN!;
table_definition = CREATE!, COLLECTION!, NAME:IDENTIFIER, COLUMNS:collection_element_list;
statement = STATEMENT:table_definition;
";

    /// <summary>
    /// Returns a visitor object suitable for parsing Sqlish grammar.
    /// </summary>
    /// <returns></returns>
    private Visitor<SqlishState> SqlishVisitor
    {
        get
        {
            var visitor = new Visitor<SqlishState>(new SqlishState());
            return visitor;
        }
    }

    [Fact]
    public void CheckProductionRules()
    {
        var parser = new Parser(QueryLanguageGrammar, "statement");
        var productionRules = parser.ProductionRules;
        Assert.Equal(23, productionRules.Count());
    }

    /// <summary>
    /// This test applies a Sqlish query to a dataset,
    /// and checks the correct number of rows are returned back.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="expectedRows"></param>
    [Theory]
    [InlineData("CREATE COLLECTION MyCollection ( a int NOT NULL , b TEXT , c DATETIME )")]
    public void TestValidSyntax(string input)
    {
        var parser = new Parser(QueryLanguageGrammar, "statement");
        var ast = parser.Parse(input);
        var pp = ast.PrettyPrint();
    }
}
