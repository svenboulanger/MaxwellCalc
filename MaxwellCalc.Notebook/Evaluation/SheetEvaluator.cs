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
    /// Evaluates a single line against the (live, being-threaded) workspace. A line whose first
    /// non-whitespace character is the text marker (<c>#</c>) is prose with inline <c>{…}</c>
    /// expressions; anything else is a single expression.
    /// </summary>
    private static LineResult EvaluateLine<T>(IWorkspace<T> workspace, string text, string? format)
        where T : struct, IFormattable
    {
        if (text.Trim().Length == 0)
            return LineResult.Empty;

        if (TextLineParser.TryGetMarker(text, out int markerIndex))
            return EvaluateTextLine(workspace, text, markerIndex, format);

        return EvaluateExpression(workspace, text, format);
    }

    /// <summary>
    /// Evaluates a prose line: literal spans pass through verbatim (un-escaped), and each inline
    /// <c>{…}</c> expression is evaluated against the live workspace via <see cref="EvaluateExpression"/>.
    /// Because the pass threads one workspace, an inline assignment or function definition binds into
    /// the transient scope and is visible to later segments and later lines (and rolled back at pass end).
    /// </summary>
    private static LineResult EvaluateTextLine<T>(IWorkspace<T> workspace, string text, int markerIndex, string? format)
        where T : struct, IFormattable
    {
        int contentStart = TextLineParser.GetContentStart(text, markerIndex);
        var segments = new List<TextSegment>();

        foreach (var span in TextLineParser.ScanSegments(text, contentStart))
        {
            if (span.Kind == TextSpanKind.Literal)
            {
                segments.Add(new TextSegment(
                    TextLineParser.Unescape(text.Substring(span.Start, span.Length)), null, string.Empty));
                continue;
            }

            // Expression span: [Start, Start+Length) includes the surrounding braces.
            string raw = text.Substring(span.Start, span.Length);
            string inner = text.Substring(span.Start + 1, span.Length - 2);

            // An empty/whitespace {} carries nothing to evaluate — show the braces as literal text.
            if (inner.Trim().Length == 0)
                segments.Add(new TextSegment(raw, null, string.Empty));
            else
                segments.Add(new TextSegment(null, EvaluateExpression(workspace, inner, format), raw));
        }

        return LineResult.Text(segments);
    }

    /// <summary>
    /// Evaluates one expression string against the (live, being-threaded) workspace, returning a
    /// Value / Assign / FuncDef / Error result. Shared by ordinary expression lines and by each inline
    /// <c>{…}</c> segment of a text line.
    /// </summary>
    private static LineResult EvaluateExpression<T>(IWorkspace<T> workspace, string text, string? format)
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
                string signature = $"{function.Name}({string.Join(", ", parameters)})";
                return new LineResult(LineKind.FuncDef, default, false, false, null, signature);
            }

            string? assignedName = node is BinaryNode { Type: BinaryOperatorTypes.Assign, Left: VariableNode assignedVariable }
                ? assignedVariable.Content.ToString()
                : null;
            bool isAssignment = assignedName is not null;

            // Resolve and format. For an assignment this also binds the variable into the (transient)
            // scope so later lines can reference it.
            if (!TryResolveOnce(workspace, node, out var quantity, out bool autoUnitSelected) ||
                !workspace.Resolver.TryFormat(quantity, format, CultureInfo.InvariantCulture, out var formatted) ||
                diagnostics.Count > 0)
            {
                return LineResult.Error(JoinDiagnostics(diagnostics) ?? "Could not evaluate the expression.");
            }

            // A line that is a single constant identifier gets the 'const' pill.
            bool isConstBadge = !isAssignment
                && node is VariableNode variable
                && workspace.Constants.Local.ContainsKey(variable.Content.ToString());

            autoUnitSelected &= !isAssignment;

            return new LineResult(
                isAssignment ? LineKind.Assign : LineKind.Value,
                formatted,
                isConstBadge,
                autoUnitSelected,
                null,
                assignedName);
        }
        finally
        {
            workspace.DiagnosticMessagePosted -= StoreDiagnostic;
        }
    }

    /// <summary>
    /// Resolves a node exactly once, and determines along the way whether the workspace auto-selected
    /// an output unit for it (the "value ≥ 1" rule, used by the Step 6 auto-caption).
    /// <para>
    /// The node must only be resolved once: resolving has side effects (it updates the answer variable,
    /// and binds any assignments embedded in the expression), so a second resolution would e.g. make
    /// <c>ans*2</c> double <c>ans</c> twice. To still observe the auto-selection, the node is resolved
    /// in base units and the output-unit step that <see cref="IWorkspace{T}.TryResolve"/> would otherwise
    /// apply is done here instead.
    /// </para>
    /// </summary>
    private static bool TryResolveOnce<T>(IWorkspace<T> workspace, INode node, out Quantity<T> result, out bool autoUnitSelected)
        where T : struct, IFormattable
    {
        autoUnitSelected = false;

        // An explicit "a in b" conversion is never an auto-selection, and is fully handled by the workspace.
        if (node is BinaryNode { Type: BinaryOperatorTypes.InUnit })
            return workspace.TryResolve(node, out result);

        bool resolveOutputUnits = workspace.ResolveOutputUnits;
        workspace.ResolveOutputUnits = false;
        Quantity<T> raw;
        try
        {
            if (!workspace.TryResolve(node, out raw))
            {
                result = default;
                return false;
            }
        }
        finally
        {
            workspace.ResolveOutputUnits = resolveOutputUnits;
        }

        // Mirror the output-unit step of the workspace's own resolution.
        result = raw;
        if (workspace.AllowUnits && resolveOutputUnits)
            autoUnitSelected = workspace.TryResolveOutputUnits(raw, out result) && result.Unit != raw.Unit;
        return true;
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
