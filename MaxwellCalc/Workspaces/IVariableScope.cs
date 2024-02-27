using MaxwellCalc.Units;

namespace MaxwellCalc.Workspaces
{
    /// <summary>
    /// Represents a scope of variables.
    /// </summary>
    public interface IVariableScope
    {
        /// <summary>
        /// Determines whether a string represents a variable that is present in the current scope.
        /// </summary>
        /// <param name="name">The variable name.</param>
        /// <returns>Returns <c>true</c> if the name represents a variable; otherwise, <c>false</c>.</returns>
        public bool IsVariable(string name);
    }

    /// <summary>
    /// Represents a variable scope.
    /// </summary>
    public interface IVariableScope<T> : IVariableScope
    {
        /// <summary>
        /// Tries to get a variable value from the workspace.
        /// </summary>
        /// <param name="name">The name of the variable.</param>
        /// <param name="result">The result.</param>
        /// <returns>Returns <c>true</c> if the variable was found; otherwise, <c>false</c>.</returns>
        public bool TryGetVariable(string name, out Quantity<T> result);

        /// <summary>
        /// Tries to update a variable value from the workspace.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        /// <returns>Returns <c>true</c> if the variable was updated; otherwise, <c>false</c>.</returns>
        public bool TrySetVariable(string name, Quantity<T> value);

        /// <summary>
        /// Creates a local scope.
        /// </summary>
        /// <returns>The variable scope.</returns>
        public IVariableScope<T> CreateLocal();
    }
}
