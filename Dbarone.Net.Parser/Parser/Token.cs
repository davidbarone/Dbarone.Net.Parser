namespace Dbarone.Net.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// A terminal token within the input. The lexer converts string input into an array of lexed tokens.
/// </summary>
public class Token
{
    /// <summary>
    /// Describes the token.
    /// </summary>
    public string TokenName { get; set; } = default!;

    /// <summary>
    /// The actual value of the token.
    /// </summary>
    public string TokenValue { get; set; } = default!;

    
}
