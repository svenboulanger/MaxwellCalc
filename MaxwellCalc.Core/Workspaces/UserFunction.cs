using MaxwellCalc.Core.Parsers.Nodes;
using System.Text.Json.Serialization;

namespace MaxwellCalc.Core.Workspaces;

/// <summary>
/// A user-defined function.
/// </summary>
public readonly record struct UserFunction
{
    /// <summary>
    /// Gets the function parameters.
    /// </summary>
    [JsonPropertyName("p")]
    public string[] Parameters { get; }

    /// <summary>
    /// Gets the (compiled) body.
    /// </summary>
    [JsonPropertyName("b")]
    public INode[] Body { get; }

    /// <summary>
    /// Creates a new <see cref="UserFunction"/>.
    /// </summary>
    /// <param name="parameters">The function parameters.</param>
    /// <param name="body">The function body.</param>
    [JsonConstructor]
    public UserFunction(string[] parameters, INode[] body)
    {
        Parameters = parameters;
        Body = body;
    }
}
