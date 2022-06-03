namespace Dbarone.Net.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Defines the type of production rule.
/// </summary>
public enum RuleType
{
    /// <summary>
    /// A terminal symbol or lexer rule. Lexer rules must start with an uppercase character.
    /// </summary>
    LexerRule,

    /// <summary>
    /// A non-terminal or parser rule. Parser rules must start with a lowercase character.
    /// </summary>
    ParserRule
}
