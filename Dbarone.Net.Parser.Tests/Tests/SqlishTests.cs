namespace Dbarone.Net.Parser.Tests;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Xunit;

public class Customer
{
    public string Name { get; set; } = default!;
    public int Age { get; set; } = default!;
    public string Country { get; set; } = default!;
    public string Sex { get; set; } = default!;
    public string Rating { get; set; } = default!;
}

public class SqlishTests : AbstractTests
{
    List<Customer> data = new List<Customer>(){
        new Customer{ Name = "john", Age= 40, Country= "Australia", Sex = "M", Rating = "A"},
        new Customer{ Name= "peter", Age= 23, Country= "UK", Sex= "M", Rating= "C" },
        new Customer{ Name= "fred", Age= 42, Country= "USA", Sex= "M", Rating= "A" },
        new Customer{ Name= "ian", Age= 71, Country= "France", Sex= "M", Rating= "B" },
        new Customer{ Name= "tony", Age= 18, Country= "Canada", Sex= "M", Rating= "B" },
        new Customer{ Name= "mark", Age= 35, Country= "Germany", Sex= "M", Rating= "C" },
        new Customer{ Name= "david", Age= 37, Country= "Italy", Sex= "M", Rating= "C" },
        new Customer{ Name= "jane", Age= 52, Country= "USA", Sex= "F", Rating= "" },
        new Customer{ Name= "sarah", Age= 55, Country= "UK", Sex= "F", Rating= "A" },
        new Customer{ Name= "sue", Age= 61, Country= "Italy", Sex= "F", Rating= "C" },
        new Customer{ Name= "alice", Age= 76, Country= "France", Sex= "F", Rating= "B" },
        new Customer{ Name= "karen", Age= 39, Country= "Australia", Sex= "F", Rating= "C" },
        new Customer{ Name= "kate", Age= 26, Country= "Germany", Sex= "F", Rating= "A" },
        new Customer{ Name= "lucy", Age= 46, Country= "Australia", Sex= "F", Rating= "A" },
        new Customer{ Name= "brian", Age= 30, Country= "UK", Sex= "M", Rating= "C" },
        new Customer{ Name= "paul", Age= 49, Country= "USA", Sex= "M", Rating= "C" }
    };

    /// <summary>
    /// Defines the grammar of Sqlish - our "pseudo SQL" language.
    /// </summary>
    public string SqlishGrammar => @"

/* Lexer Rules */

AND             = ""\bAND\b"";
OR              = ""\bOR\b"";
EQ_OP           = ""\bEQ\b"";
NE_OP           = ""\bNE\b"";
LT_OP           = ""\bLT\b"";
LE_OP           = ""\bLE\b"";
GT_OP           = ""\bGT\b"";
GE_OP           = ""\bGE\b"";
LEFT_PAREN      = ""[(]"";
RIGHT_PAREN     = ""[)]"";
COMMA           = "","";
IN              = ""\b(IN)\b"";
CONTAINS        = ""\bCONTAINS\b"";
BETWEEN         = ""\bBETWEEN\b"";
ISBLANK         = ""\bISBLANK\b"";
NOT             = ""\bNOT\b"";
LITERAL_STRING  = ""['][^']*[']"";
LITERAL_NUMBER  = ""[+-]? ((\d+(\.\d*)?)|(\.\d+))"";
IDENTIFIER      = ""[A-Z_][A-Z_0-9]*"";
WHITESPACE      = ""\s+"";

/*Parser Rules */

comparison_operator =   :EQ_OP | :NE_OP | :LT_OP | :LE_OP | :GT_OP | :GE_OP;
comparison_operand  =   :LITERAL_STRING | :LITERAL_NUMBER | :IDENTIFIER;
comparison_predicate=   LHV:comparison_operand, OPERATOR:comparison_operator, RHV:comparison_operand;
in_factor           =   COMMA!, :comparison_operand;
in_predicate        =   LHV:comparison_operand, NOT:NOT?, IN!, LEFT_PAREN!, RHV:comparison_operand, RHV:in_factor*, RIGHT_PAREN!;
between_predicate   =   LHV:comparison_operand, NOT:NOT?, BETWEEN!, OP1:comparison_operand, AND!, OP2:comparison_operand;
contains_predicate  =   LHV:comparison_operand, NOT:NOT?, CONTAINS!, RHV:comparison_operand;
blank_predicate     =   LHV:comparison_operand, NOT:NOT?, ISBLANK;
predicate           =   :comparison_predicate | :in_predicate | :between_predicate | :contains_predicate | :blank_predicate;
boolean_primary     =   :predicate;
boolean_primary     =   LEFT_PAREN!, CONDITION:search_condition, RIGHT_PAREN!;
boolean_factor      =   AND!, :boolean_primary;
boolean_term        =   AND:boolean_primary, AND:boolean_factor*;
search_factor       =   OR!, :boolean_term;
search_condition    =   OR:boolean_term, OR:search_factor*;";

    /// <summary>
    /// Returns a visitor object suitable for parsing Sqlish grammar.
    /// </summary>
    /// <returns></returns>
    private Visitor SqlishVisitor
    {
        get
        {
            // Initial state
            dynamic state = new ExpandoObject();
            state.Stack = new Stack<int>();
            state.FilterFunctions = new Stack<Func<Customer, bool>>();
            var visitor = new Visitor(state);

            visitor.AddVisitor(
                "search_condition",
                (v, n) =>
                {
                    var funcs = new Stack<Func<Customer, bool>>();

                    dynamic searchCondition = n.Properties["OR"];
                    foreach (var item in (IEnumerable<Object>)searchCondition)
                    {
                        var node = item as Node;
                        if (node == null)
                            throw new Exception("Array element type not Node.");
                        node.Accept(v);
                        funcs.Push(v.State.FilterFunctions.Pop());
                    }

                    Func<Customer, bool> func = (row) =>
                    {
                        bool match = false;
                        foreach (var func in funcs)
                        {
                            if (func(row))
                            {
                                match = true;
                                break;
                            }
                        }
                        return match;
                    };

                    v.State.FilterFunctions.Push(func);
                }
            );

            visitor.AddVisitor(
                "boolean_term",
                (v, n) =>
                {
                    var funcs = new Stack<Func<Customer, bool>>();

                    foreach (var item in (IEnumerable<Object>)n.Properties["AND"])
                    {
                        var node = item as Node;
                        if (node == null)
                            throw new Exception("Array element type not Node.");
                        node.Accept(v);
                        funcs.Push(v.State.FilterFunctions.Pop());
                    }

                    Func<Customer, bool> func = (row) =>
                    {
                        bool match = true;
                        foreach (var func in funcs)
                        {
                            if (!func(row))
                            {
                                match = false;
                                break;
                            }
                        }
                        return match;
                    };

                    v.State.FilterFunctions.Push(func);

                }
            );

            visitor.AddVisitor(
                "boolean_primary",
                (v, n) =>
                {
                    // If CONDITION property present, then need to wrap () around condition.
                    if (n.Properties.ContainsKey("CONDITION"))
                    {
                        var node = n.Properties["CONDITION"] as Node;
                        if (node == null)
                            throw new Exception("Array element type not Node.");

                        node.Accept(v);
                        var innerFilter = v.State.FilterFunctions.Pop();
                        v.State.FilterFunctions.Push(innerFilter);
                    }
                }
            );

            visitor.AddVisitor(
                "comparison_predicate",
                (v, n) =>
                {
                    bool not = n.Properties.ContainsKey("NOT");
                    string column = (n.Properties["LHV"] as Token)!.TokenValue;
                    string[] values = (n!.Properties["LHV"] as string[])!;

                    string operatorTokenName = (n.Properties["OPERATOR"] as Token)!.TokenName;
                    string value = (n.Properties["RHV"] as Token)!.TokenValue.Replace("'", "");

                    Func<Customer, bool> func = (row) =>
                    {
                        bool match = false;
                        var pi = row.GetType().GetProperty(column)!;
                        switch (operatorTokenName)
                        {
                            case "EQ_OP":
                                match = pi.GetValue(row) == value;
                                break;
                            case "NE_OP":
                                match = pi.GetValue(row) != value;
                                break;
                            case "LT_OP":
                                match = (pi.GetValue(row) as IComparable)!.CompareTo(value) < 0;
                                break;
                            case "LE_OP":
                                match = (pi.GetValue(row) as IComparable)!.CompareTo(value) <= 0;
                                break;
                            case "GT_OP":
                                match = (pi.GetValue(row) as IComparable)!.CompareTo(value) > 0;
                                break;
                            case "GE_OP":
                                match = (pi.GetValue(row) as IComparable)!.CompareTo(value) >= 0;
                                break;
                        }
                        return match;
                    };

                    v.State.FilterFunctions.Push(func);
                }
            );

            visitor.AddVisitor(
                    "in_predicate",
                    (v, n) =>
                    {
                        bool not = n.Properties.ContainsKey("NOT");
                        string column = (n.Properties["LHV"] as Token)!.TokenValue;
                        string[] values = (n.Properties["RHV"] as string[])!;
                        values = values.Select(v => v.Replace("'", "")).ToArray();

                        Func<Customer, bool> func = (row) =>
                        {
                            var pi = row.GetType().GetProperty(column)!;
                            var includes = values.Contains(pi.GetValue(row));
                            return includes;
                        };

                        Func<Customer, bool> funcNot = (row) =>
                        {
                            var pi = row.GetType().GetProperty(column)!;
                            var includes = values.Contains(pi.GetValue(row));
                            return !includes;
                        };

                        if (not)
                            v.State.FilterFunctions.Push(funcNot);
                        else
                            v.State.FilterFunctions.Push(func);
                    }
                );

            visitor.AddVisitor(
                    "between_predicate",
                    (v, n) =>
                    {
                        bool not = n.Properties.ContainsKey("NOT");
                        string column = (n.Properties["LHV"] as Token)!.TokenValue;
                        object op1 = (n.Properties["OP1"]! as Token)!.TokenValue.Replace(@"""", "");
                        object op2 = (n.Properties["OP2"]! as Token)!.TokenValue.Replace(@"""", "");

                        Func<Customer, bool> func = (row) =>
                        {
                            var pi = row.GetType().GetProperty(column)!;
                            var result = (pi.GetValue(row) as IComparable)!.CompareTo(op1) >= 0 && (pi.GetValue(row) as IComparable)!.CompareTo(op2) <= 0;
                            return result;
                        };

                        Func<Customer, bool> funcNot = (row) =>
                        {
                            var pi = row.GetType().GetProperty(column)!;
                            var result = (pi.GetValue(row) as IComparable)!.CompareTo(op1) >= 0 && (pi.GetValue(row) as IComparable)!.CompareTo(op2) <= 0;
                            return !result;
                        };

                        if (not)
                            v.State.FilterFunctions.Push(funcNot);
                        else
                            v.State.FilterFunctions.Push(func);
                    }
                );

            visitor.AddVisitor(
                "contains_predicate",
                (v, n) =>
                {
                    bool not = n.Properties.ContainsKey("NOT");
                    string column = (n.Properties["LHV"] as Token)!.TokenValue;
                    string value = (n.Properties["RHV"]! as Token)!.TokenValue.Replace(@"""", "");

                    Func<Customer, bool> func = (row) =>
                    {
                        var pi = row.GetType().GetProperty(column)!;
                        var result = (pi.GetValue(row) as string)!.Contains(value);
                        return result;
                    };

                    Func<Customer, bool> funcNot = (row) =>
                    {
                        var pi = row.GetType().GetProperty(column)!;
                        var result = (pi.GetValue(row) as string)!.Contains(value);
                        return !result;
                    };

                    if (not)
                        v.State.FilterFunctions.Push(funcNot);
                    else
                        v.State.FilterFunctions.Push(func);
                }
            );

            visitor.AddVisitor(
                "blank_predicate",
                (v, n) =>
                {
                    bool not = n.Properties.ContainsKey("NOT");
                    string column = (n.Properties["LHV"] as Token)!.TokenValue;

                    Func<Customer, bool> func = (row) =>
                    {
                        var pi = row.GetType().GetProperty(column)!;
                        var result = string.IsNullOrEmpty(pi.GetValue(row) as string);
                        return result;
                    };

                    Func<Customer, bool> funcNot = (row) =>
                    {
                        var pi = row.GetType().GetProperty(column)!;
                        var result = string.IsNullOrEmpty(pi.GetValue(row) as string);
                        return !result;
                    };

                    if (not)
                        v.State.FilterFunctions.Push(funcNot);
                    else
                        v.State.FilterFunctions.Push(func);
                }
            );

            return visitor;
        }
    }

    private int StateMapper(dynamic state)
    {
        Func<Customer, bool> func = (state.filterFunctions as Stack<Func<Customer, bool>>)!.Pop();
        var filtered = data.Where(r => func(r));
        return filtered.Count();
    }

    [Fact]
    public void CheckProductionRules(){
        var parser = new Parser(SqlishGrammar, "search_condition");
        var productionRules = parser.ProductionRules;
        Assert.Equal(46, productionRules.Count());

    }

    [Theory]
    [InlineData("age BETWEEN 40 AND 60", 6)]
    public void TestSqlish(string input, int expectedRows) {
        var parser = new Parser(SqlishGrammar, "search_condition");
        var ast = parser.Parse(input);
        var visitor = SqlishVisitor;
        ast.Walk(visitor);
        var actualRows = StateMapper(visitor);
        Assert.Equal(expectedRows, actualRows);
    }
}
