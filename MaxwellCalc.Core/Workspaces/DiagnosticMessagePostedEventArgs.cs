using System;

namespace MaxwellCalc.Core.Workspaces;

/// <summary>
/// A diagnostic message event argument.
/// </summary>
/// <param name="message">The message.</param>
public class DiagnosticMessagePostedEventArgs(string message) : EventArgs
{
    /// <summary>
    /// Gets the message.
    /// </summary>
    public string Message { get; } = message;
}
