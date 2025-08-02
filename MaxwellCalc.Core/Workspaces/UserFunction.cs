using System.Text.Json.Serialization;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// A user-defined function.
    /// </summary>
    public readonly struct UserFunction
    {
        /// <summary>
        /// Gets the name.
        /// </summary>
        [JsonPropertyName("n")]
        public string Name { get; }

        /// <summary>
        /// Gets the function parameters.
        /// </summary>
        [JsonPropertyName("p")]
        public string[] Parameters { get; }

        /// <summary>
        /// Gets the (compiled) body.
        /// </summary>
        [JsonPropertyName("b")]
        public string Body { get; }

        /// <summary>
        /// Creates a new <see cref="UserFunction"/>.
        /// </summary>
        /// <param name="name">The function name.</param>
        /// <param name="parameters">The function parameters.</param>
        /// <param name="body">The function body.</param>
        [JsonConstructor]
        public UserFunction(string name, string[] parameters, string body)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }
    }
}
