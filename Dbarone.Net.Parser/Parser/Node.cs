namespace Dbarone.Net.Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// <summary>
/// Represents a non-leaf node of the abstract syntax tree.
/// </summary>
public class Node
{
    /// <summary>
    /// Name of the node. Equivalent to the name of the symbol it matches, or its alias.
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Properties (children) of the node / production rule.
    /// </summary>
    public Dictionary<string, object> Properties = new Dictionary<string, object>();

    /// <summary>
    /// Constructor for create a new node.
    /// </summary>
    /// <param name="name">The name of the symbol or production rule.</param>
    public Node(string name)
    {
        this.Name = name;
    }

    /// <summary>
    /// Accepts a visitor on this node.
    /// </summary>
    /// <param name="v"></param>
    public void Accept<TState>(Visitor<TState> v)
    {
        v.Visit(this);
    }

    /// <summary>
    /// Navigates the abstract syntax tree.
    /// </summary>
    /// <param name="v">The visitor to use to walk the tree.</param>
    public void Walk<TState>(Visitor<TState> v)
    {
        this.Accept(v);
        var state = v.State;
    }

    /// <summary>
    /// Pretty-prints a node's abstract syntax tree.
    /// </summary>
    /// <param name="indent">The current indent.</param>
    /// <param name="isLastChild">Set to true if the last child in a node.</param>
    /// <returns>Text representation of the node.</returns>
    /// <exception cref="Exception"></exception>
    public string PrettyPrint(string indent = "", bool isLastChild = true)
    {
        var output = $"{indent}+- {this.Name}{Environment.NewLine}";
        indent += isLastChild ? "   " : "|  ";

        // print children too
        List<string> keys = this.Properties.Keys.ToList();

        for (int i = 0; i < keys.Count(); i++)
        {
            var key = keys[i];
            var child = this.Properties[key];
            var childAsIEnumerable = child as IEnumerable<object>;
            if (childAsIEnumerable != null)
            {
                // special array identifier
                output += $"{indent}+- \"{key}\" []{Environment.NewLine}";
                indent += "   ";

                foreach (var item in childAsIEnumerable)
                {
                    if (item as Token != null)
                    {
                        // token
                        output += $"{indent}+- {(item as Token)!.TokenName} [{(item as Token)!.TokenValue}]{Environment.NewLine}";
                    }
                    else if (item as Node != null)
                    {
                        // node
                        output += (item as Node)!.PrettyPrint(indent, i == keys.Count() - 1 && item == childAsIEnumerable.Last());
                    }
                    else
                    {
                        // throw
                        throw new Exception("Node.PrettyPrint(): item must be Node or Token (1).");
                    }
                }
            }
            else
            {
                if (child as Token != null)
                {
                    // token
                    output += $"{indent}+- \"{key}\" {(child as Token)!.TokenName} [{(child as Token)!.TokenValue}]{Environment.NewLine}";
                }
                else if (child as Node != null)
                {
                    // node
                    output += (child as Node)!.PrettyPrint(indent, i == keys.Count() - 1);
                }
                else
                {
                    // throw
                    throw new Exception("Node.PrettyPrint(): item must be Node or Token (1).");
                }
            }
        }
        return output;
    }

    #region Helper Methods

    public Node GetNode(string propertyName)
    {
        var node = this.Properties[propertyName] as Node;
        if (node == null)
        {
            throw new Exception($"Property [{propertyName}] is not a Node type.");
        }
        return node;
    }

    public Token GetToken(string propertyName)
    {
        var token = this.Properties[propertyName] as Token;
        if (token == null)
        {
            throw new Exception($"Property [{propertyName}] is not a Token type.");
        }
        return token;
    }

    public Node[] GetNodeArray(string propertyName)
    {
        var obj = this.Properties[propertyName] as IEnumerable<object>;
        var nodes = new List<Node>();
        foreach (var item in obj)
        {
            var node = item as Node;
            if (node == null)
            {
                throw new Exception($"Property [{propertyName}] must be a Node array. Not all elements are Node types.");
            }
            nodes.Add(node);
        }
        return nodes.ToArray();
    }

    public Token[] GetTokenArray(string propertyName)
    {
        var obj = this.Properties[propertyName] as IEnumerable<object>;
        var tokens = new List<Token>();
        foreach (var item in obj)
        {
            var token = item as Token;
            if (token == null)
            {
                throw new Exception($"Property [{propertyName}] must be a Token array. Not all elements are Token types.");
            }
            tokens.Add(token);
        }
        return tokens.ToArray();
    }

    #endregion
}
