using MaxwellCalc.Core.Parsers;
using MaxwellCalc.Core.Parsers.Nodes;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Core.Workspaces.Variables;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace MaxwellCalc.Notebook.Evaluation;

/// <summary>
/// Evaluates a notebook sheet — a list of line texts — against a workspace, top to bottom,
/// threading a running scope so later lines see earlier assignments and function definitions.
/// <para>
/// The evaluation is <b>transient</b>: assignments and function definitions made on the sheet are
/// visible to later lines within the same pass, but the persistent workspace is left exactly as it
/// was afterwards. This is achieved by snapshotting the workspace's variable scope and user functions
/// before the pass and restoring them in a <c>finally</c>. The persistent workspace therefore only
/// ever changes through the overlay's explicit add/remove commands (Step 9), never as the user types.
/// </para>
/// <para>
/// This class is intentionally free of any UI dependency so it can be unit-tested directly.
/// </para>
/// </summary>
public static class SheetEvaluator
{
    /// <summary>
    /// Evaluates each line of the sheet against the workspace without mutating it.
    /// </summary>
    /// <param name="workspace">The active workspace, or <c>null</c>.</param>
    /// <param name="lines">The raw line texts, in order.</param>
    /// <param name="format">The numeric format handed to the workspace formatter.</param>
    /// <returns>Returns one <see cref="LineResult"/> per input line, in the same order.</returns>
    public static IReadOnlyList<LineResult> Evaluate(IWorkspace? workspace, IReadOnlyList<string> lines, string? format = null)
    {
        var results = new LineResult[lines.Count];

        // Unknown/absent workspace (or an unsupported scalar type): every line is left empty.
        switch (workspace)
        {
            case IWorkspace<double> real:
                EvaluatePass(real, lines, results, format);
                break;

            case IWorkspace<System.Numerics.Complex> complex:
                EvaluatePass(complex, lines, results, format);
                break;
        }

        return results;
    }

    /// <summary>
    /// Runs a single evaluation pass over a typed workspace, snapshotting and restoring the
    /// workspace state so the pass leaves no trace.
    /// </summary>
    private static void EvaluatePass<T>(IWorkspace<T> workspace, IReadOnlyList<string> lines, LineResult[] results, string? format)
        where T : struct, IFormattable
    {
        var variableScope = (IVariableScope<T>)workspace.Variables;

        // Snapshot everything a pass could mutate: local variables (assignments + the answer
        // variable) and user functions (function definitions).
        var savedVariables = new Dictionary<string, Variable<T>>(variableScope.Local);
        var savedFunctions = new Dictionary<UserFunctionKey, UserFunction>(workspace.UserFunctions);

        try
        {
            for (int i = 0; i < lines.Count; i++)
                results[i] = EvaluateLine(workspace, lines[i], format);
        }
        finally
        {
            RestoreDictionary(variableScope.Local, savedVariables);
            RestoreDictionary(workspace.UserFunctions, savedFunctions);
        }
    }

    /// <summary>
    /// Evaluates a single line against the (live, being-threaded) workspace.
    /// </summary>
    private static LineResult EvaluateLine<T>(IWorkspace<T> workspace, string text, string? format)
        where T : struct, IFormattable
    {
        string trimmed = text.Trim();
        if (trimmed.Length == 0)
            return LineResult.Empty;

        var diagnostics = new List<string>();
        void StoreDiagnostic(object? sender, DiagnosticMessagePostedEventArgs args) => diagnostics.Add(args.Message);
        workspace.DiagnosticMessagePosted += StoreDiagnostic;

        try
        {
            var lexer = new Lexer(trimmed);
            var node = Parser.Parse(lexer, workspace);
            if (node is null)
                return LineResult.Error(JoinDiagnostics(diagnostics) ?? "Could not parse the expression.");

            // Function definition: f(x) = <body>. We register this ourselves rather than letting the
            // workspace resolve the assignment, because the workspace's assign path does not capture
            // the parameter names. Registering directly threads the definition to later lines and is
            // undone when the pass restores the user-function snapshot.
            if (node is BinaryNode { Type: BinaryOperatorTypes.Assign, Left: FunctionNode function } functionAssign)
            {
                var parameters = new List<string>(function.Arguments.Count);
                foreach (var argument in function.Arguments)
                {
                    if (argument is not VariableNode parameter)
                        return LineResult.Error("A function argument has to be a simple variable.");
                    parameters.Add(parameter.Content.ToString());
                }

                workspace.UserFunctions[new UserFunctionKey(function.Name, parameters.Count)] =
                    new UserFunction([.. parameters], [functionAssign.Right]);
                return new LineResult(LineKind.FuncDef, default, false, false, null);
            }

            bool isAssignment = node is BinaryNode { Type: BinaryOperatorTypes.Assign, Left: VariableNode };

            // Resolve and format. For an assignment this also binds the variable into the (transient)
            // scope so later lines can reference it.
            if (!workspace.TryResolveAndFormat(node, format, CultureInfo.InvariantCulture, out var formatted) ||
                diagnostics.Count > 0)
            {
                return LineResult.Error(JoinDiagnostics(diagnostics) ?? "Could not evaluate the expression.");
            }

            // A line that is a single constant identifier gets the 'const' pill.
            bool isConstBadge = !isAssignment
                && node is VariableNode variable
                && workspace.Constants.Local.ContainsKey(variable.Content.ToString());

            bool autoUnitSelected = !isAssignment && DetectAutoUnitSelected(workspace, node, StoreDiagnostic);

            return new LineResult(
                isAssignment ? LineKind.Assign : LineKind.Value,
                formatted,
                isConstBadge,
                autoUnitSelected,
                null);
        }
        finally
        {
            workspace.DiagnosticMessagePosted -= StoreDiagnostic;
        }
    }

    /// <summary>
    /// Determines whether the workspace auto-selected an output unit for this line (the "value ≥ 1"
    /// rule). This is only meaningful for expressions that did not explicitly request a unit with the
    /// <c>in</c> operator. Used by the Step 6 auto-caption; captured here so the data model is complete.
    /// </summary>
    private static bool DetectAutoUnitSelected<T>(IWorkspace<T> workspace, INode node, EventHandler<DiagnosticMessagePostedEventArgs> diagnosticHandler)
        where T : struct, IFormattable
    {
        // An explicit "a in b" conversion is never an auto-selection.
        if (node is BinaryNode { Type: BinaryOperatorTypes.InUnit })
            return false;

        // Probe the raw (base-unit) quantity without letting the probe's diagnostics leak into the
        // line result, and without converting to an output unit.
        workspace.DiagnosticMessagePosted -= diagnosticHandler;
        var oldState = workspace.Restrict(
            resolveInputUnits: workspace.ResolveInputUnits,
            resolveOutputUnits: false,
            allowUnits: workspace.AllowUnits,
            allowVariables: workspace.AllowVariables,
            allowUserFunctions: workspace.AllowUserFunctions);
        try
        {
            if (!workspace.TryResolve(node, out var raw))
                return false;
            if (raw.Unit.Dimension is null || raw.Unit.Dimension.Count == 0)
                return false;

            return workspace.TryResolveOutputUnits(raw, out var converted) && converted.Unit != raw.Unit;
        }
        finally
        {
            workspace.Restore(oldState);
            workspace.DiagnosticMessagePosted += diagnosticHandler;
        }
    }

    /// <summary>
    /// Restores an observable dictionary to a previously captured snapshot: removes entries added
    /// during the pass and re-applies the snapshot values (record-struct equality means unchanged
    /// entries fire no events).
    /// </summary>
    private static void RestoreDictionary<TKey, TValue>(
        Core.Dictionaries.IObservableDictionary<TKey, TValue> dictionary,
        Dictionary<TKey, TValue> snapshot)
        where TKey : notnull
    {
        foreach (var key in dictionary.Keys.ToList())
        {
            if (!snapshot.ContainsKey(key))
                dictionary.Remove(key);
        }

        foreach (var pair in snapshot)
            dictionary[pair.Key] = pair.Value;
    }

    private static string? JoinDiagnostics(List<string> diagnostics)
        => diagnostics.Count > 0 ? string.Join(Environment.NewLine, diagnostics) : null;
}
