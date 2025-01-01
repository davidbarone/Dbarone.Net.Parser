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
TRUE = ""\bTRUE\b"";
FALSE = ""\bFALSE\b"";

CREATETABLE = ""\bCREATETABLE\b"";
CREATERELATION = ""\bCREATERELATION\b"";
CREATESEQUENCE = ""\bCREATESEQUENCE\b"";
LEFT_PAREN = ""[(]"";
RIGHT_PAREN = ""[)]"";
COMMA = "","";
IDENTIFIER      = ""[A-Z_][A-Z_0-9]*"";

/* Parser rules */

true_false = :TRUE | FALSE;
column_name = :IDENTIFIER;
null = :NULL;
not_null = :NOT,:NULL;
null_specifier = :(:null | :not_null);
data_type = :DATA_TYPE_INTEGER | :DATA_TYPE_REAL | :DATA_TYPE_TEXT | :DATA_TYPE_DATETIME;

/* CREATETABLE */
column_definition = COLUMN_NAME:column_name, COMMA!, DATA_TYPE:data_type, COMMA!, NULL_SPECIFIER:null_specifier;
collection_element_list = :column_definition, :(COMMA!, :column_definition)*;
table_definition = CREATETABLE!, LEFT_PAREN!, NAME:IDENTIFIER, COMMA!, COLUMNS:collection_element_list, RIGHT_PAREN!;

/* CREATERELATION */
relation_definition = CREATERELATION!, LEFT_PAREN!, TABLE_FROM:IDENTIFIER, COMMA!, COLUMN_FROM:IDENTIFIER, COMMA!, TABLE_TO:IDENTIFIER, COMMA!, COLUMN_TO:IDENTIFIER, RIGHT_PAREN!;

/* CREATESEQUENCE */
sequence_definition = CREATESEQUENCE!, LEFT_PAREN!, NAME:IDENTIFIER, RIGHT_PAREN!;

statement = STATEMENT:(:table_definition | :relation_definition | :sequence_definition);
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
        Assert.Equal(37, productionRules.Count());
    }

    /// <summary>
    /// This test applies a Sqlish query to a dataset,
    /// and checks the correct number of rows are returned back.
    /// </summary>
    /// <param name="input"></param>
    /// <param name="expectedRows"></param>
    [Theory]
    [InlineData("CREATETABLE(MyTable,A,INT,NULL)")]
    [InlineData("CREATETABLE (MyTable , a , int , NOT NULL )")]
    [InlineData("CREATETABLE (MyTable , a , int , NULL , b , text , NULL , c , datetime , NULL )")]
    [InlineData("CREATETABLE (MyTable , a , int ,  NOT NULL , b , TEXT, NULL , c ,  DATETIME , NULL )")]
    [InlineData("CREATERELATION(MyChildTable, a, MyReferenceTable, a)")]
    [InlineData("CREATESEQUENCE(MySeq)")]
    public void TestValidSyntax(string input)
    {
        var parser = new Parser(QueryLanguageGrammar, "statement");
        var ast = parser.Parse(input);
        var pp = ast.PrettyPrint();
    }
}
