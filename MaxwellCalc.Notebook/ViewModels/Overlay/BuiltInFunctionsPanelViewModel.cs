using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Workspaces;
using System;

namespace MaxwellCalc.Notebook.ViewModels.Overlay;

/// <summary>
/// The command palette's Built-in functions panel: the functions always available in every workspace.
/// Read-only — built-ins can't be removed. Reads <c>workspace.BuiltInFunctions</c>.
/// </summary>
public sealed class BuiltInFunctionsPanelViewModel : FilteredPanelViewModel<BuiltInFunctionItem, string, BuiltInFunction>
{
    /// <summary>
    /// Creates a new <see cref="BuiltInFunctionsPanelViewModel"/>.
    /// </summary>
    /// <param name="workspaceState">The shared workspace state.</param>
    public BuiltInFunctionsPanelViewModel(WorkspaceState workspaceState)
        : base(workspaceState)
    {
    }

    /// <inheritdoc />
    protected override IReadOnlyObservableDictionary<string, BuiltInFunction>? GetDictionary(IWorkspace workspace)
        => workspace.BuiltInFunctions.AsReadOnly();

    /// <inheritdoc />
    protected override BuiltInFunctionItem Project(string key, BuiltInFunction value)
        => new() { Signature = BuildSignature(value), Description = value.Description };

    /// <inheritdoc />
    protected override bool Matches(BuiltInFunctionItem item, string filter)
        => item.Signature.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || item.Description.Contains(filter, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    protected override int Compare(BuiltInFunctionItem a, BuiltInFunctionItem b)
        => StringComparer.OrdinalIgnoreCase.Compare(a.Signature, b.Signature);

    // Builds a readable signature from the argument-count bounds, e.g. "sin(x)", "max(x, y, …)".
    private static string BuildSignature(BuiltInFunction function)
    {
        if (function.MinimumArgumentCount == function.MaximumArgumentCount)
        {
            var names = new string[function.MinimumArgumentCount];
            for (int i = 0; i < names.Length; i++)
                names[i] = ((char)('x' + i % 3)).ToString();
            return $"{function.Name}({string.Join(", ", names)})";
        }

        return function.MaximumArgumentCount == int.MaxValue
            ? $"{function.Name}(x, …)"
            : $"{function.Name}(x, … [{function.MinimumArgumentCount}–{function.MaximumArgumentCount}])";
    }
}
