namespace Dbarone.Net.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using Dbarone.Net.Extensions.Collections;

/// <summary>
/// Provides context during the parsing process.
/// </summary>
public class ParserContext
{
    /// <summary>
    /// The production rules in the grammar.
    /// </summary>
    public IList<ProductionRule> ProductionRules { get; private set; }

    /// <summary>
    /// The current production rule.
    /// </summary>
    public Stack<ProductionRule> CurrentProductionRule { get; set; }

    /// <summary>
    /// The tokens in the input.
    /// </summary>
    private IList<Token> Tokens { get; set; }

    /// <summary>
    /// Stack of intermediate results.
    /// </summary>
    public Stack<object> Results { get; private set; }

    /// <summary>
    /// Creates a new ParserContext object.
    /// </summary>
    /// <param name="productionRules">The list of production rules.</param>
    /// <param name="tokens">The tokenised input to parse.</param>
    public ParserContext(IList<ProductionRule> productionRules, IList<Token> tokens)
    {
        this.ProductionRules = productionRules;
        this.Tokens = tokens;
        this.CurrentTokenIndex = 0;
        this.Results = new Stack<object>();
        this.CurrentProductionRule = new Stack<ProductionRule>();
    }

    public Token PeekToken()
    {
        if (CurrentTokenIndex >= Tokens.Count())
            return new Token { TokenName = "<EOF>", TokenValue = "<EOF>" };
        else
            return Tokens[CurrentTokenIndex];
    }

    public void PushResult(object value)
    {
        Results.Push(value);
    }

    public object PopResult()
    {
        return Results.Pop();
    }

    public object PeekResult()
    {
        return Results.Peek();
    }

    /// <summary>
    /// Returns true if past the end of the token list.
    /// </summary>
    /// <returns></returns>
    public bool TokenEOF
    {
        get
        {
            return CurrentTokenIndex >= Tokens.Count();
        }
    }


    private int _bestTokenIndex = default!;

    /// <summary>
    /// Readonly property to get the best token index reached. This is to provide feedback
    /// to user if unable to fully parse input. Gets the best attempt, and returns information
    /// about tokens at this position.
    /// </summary>
    public int BestTokenIndex
    {
        get
        {
            return _bestTokenIndex;
        }
    }

    private int _currentTokenIndex = default!;

    /// <summary>
    /// Pointer to current token position.
    /// </summary>
    public int CurrentTokenIndex
    {
        get
        {
            return _currentTokenIndex;
        }
        set
        {
            _currentTokenIndex = value;
            if (value > _bestTokenIndex) { _bestTokenIndex = value; }
        }
    }

    /// <summary>
    /// Attempts to get the next token. If the next TokenName matches
    /// the tokenName parameter, the token is returned and the position
    /// is advanced by 1. Otherwise, returns null. Exception throw if
    /// EOF reached.
    /// </summary>
    /// <returns></returns>
    public Token? TryToken(string tokenName)
    {
        if (CurrentTokenIndex >= Tokens.Count)
            throw new Exception("Unexpected EOF.");
        if (tokenName.Equals(Tokens[CurrentTokenIndex].TokenName, StringComparison.OrdinalIgnoreCase))
        {
            var token = Tokens[CurrentTokenIndex];
            CurrentTokenIndex++;
            return token;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Helper method to construct the tree. Updates the result object(s) in the context.
    /// </summary>
    public void UpdateResult(string name, object value)
    {
        // only update if value is set. Possible that a symbol returns true, but
        // no match (for example if the symbol is set to optional)
        if (value != null)
        {
            var result = Results.Peek();
            var resultAsNode = (result as Node)!;  // Should always be a Node object?

            var productionRule = this.CurrentProductionRule.Peek();
            var isEnumerated = productionRule.IsEnumeratedSymbol(name);

            if (!string.IsNullOrEmpty(name))
            {
                if (isEnumerated)
                {
                    if (!resultAsNode.Properties.ContainsKey(name))
                        resultAsNode.Properties[name] = new List<object>();

                    resultAsNode.Properties[name] = resultAsNode.Properties[name].Union(value);
                }
                else
                    resultAsNode.Properties[name] = value;
            }
            else
            {
                if (isEnumerated)
                {
                    var obj = Results.Pop();
                    Results.Push(obj.Union(value));
                }
                else
                {
                    Results.Pop();
                    Results.Push(value);
                }
            }
        }
    }
}
