using System;

namespace MaxwellCalc.Core.Workspaces;

/// <summary>
/// A diagnostics handler.
/// </summary>
public interface IDiagnosticsHandler
{
    /// <summary>
    /// An event that is called when a diagnostic message is posted.
    /// </summary>
    public event EventHandler<DiagnosticMessagePostedEventArgs>? DiagnosticMessagePosted;

    /// <summary>
    /// A method that can be called to post a diagnostic message.
    /// </summary>
    /// <param name="args">The event arguments.</param>
    public void PostDiagnosticMessage(DiagnosticMessagePostedEventArgs args);
}
