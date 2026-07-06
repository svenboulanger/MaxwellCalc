using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Parsers;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Linq;

namespace MaxwellCalc.Notebook.ViewModels.Overlay;

/// <summary>
/// The command palette's Input-units panel: the symbols that resolve to base SI on input. Reads
/// <c>workspace.InputUnits</c>. The footer (Step 9) adds an input unit by symbol and its base-unit
/// definition; rows remove a unit.
/// </summary>
public sealed partial class InputUnitsPanelViewModel : FilteredPanelViewModel<InputUnitItem, string, Quantity<string>>
{
    /// <summary>Gets or sets the footer's symbol field.</summary>
    [ObservableProperty]
    private string _newSymbol = string.Empty;

    /// <summary>Gets or sets the footer's definition field (value in base units).</summary>
    [ObservableProperty]
    private string _newDefinition = string.Empty;

    /// <summary>Gets or sets the inline error shown when an add fails.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    /// <summary>Gets whether an inline error is showing.</summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Creates a new <see cref="InputUnitsPanelViewModel"/>.
    /// </summary>
    /// <param name="workspaceState">The shared workspace state.</param>
    public InputUnitsPanelViewModel(WorkspaceState workspaceState)
        : base(workspaceState)
    {
    }

    /// <summary>
    /// Adds an input unit from the footer fields. The definition is parsed under a restricted workspace
    /// (units allowed but not resolved to input/output units) then assigned via
    /// <c>TryAssignInputUnit</c>, mirroring the old <c>InputUnitsViewModel.AddInputUnit</c> flow.
    /// </summary>
    [RelayCommand]
    private void Add()
    {
        if (string.IsNullOrWhiteSpace(NewSymbol) || string.IsNullOrWhiteSpace(NewDefinition))
            return;

        string symbol = NewSymbol.Trim();
        if (!symbol.All(char.IsLetter))
        {
            ErrorMessage = "A unit symbol must contain only letters.";
            return;
        }

        ErrorMessage = RunWithDiagnostics(workspace =>
        {
            var oldState = workspace.Restrict(false, false, true, false, false);
            try
            {
                var node = Parser.Parse(new Lexer(NewDefinition), workspace);
                return node is not null && workspace.TryAssignInputUnit(symbol, node);
            }
            finally
            {
                workspace.Restore(oldState);
            }
        });

        if (ErrorMessage is null)
        {
            NewSymbol = string.Empty;
            NewDefinition = string.Empty;
        }
    }

    /// <summary>Removes an input unit (the row × button).</summary>
    /// <param name="item">The row to remove.</param>
    [RelayCommand]
    private void Remove(InputUnitItem item)
        => WorkspaceState.Workspace?.TryRemoveInputUnit(item.Symbol);

    /// <inheritdoc />
    protected override IReadOnlyObservableDictionary<string, Quantity<string>>? GetDictionary(IWorkspace workspace)
        => workspace.InputUnits;

    /// <inheritdoc />
    protected override InputUnitItem Project(string key, Quantity<string> value)
        => new() { Symbol = key, Definition = value };

    /// <inheritdoc />
    protected override bool Matches(InputUnitItem item, string filter)
        => item.Symbol.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || (item.Definition.Scalar?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false)
        || item.Definition.Unit.ToString().Contains(filter, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    protected override int Compare(InputUnitItem a, InputUnitItem b)
        => StringComparer.OrdinalIgnoreCase.Compare(a.Symbol, b.Symbol);
}
