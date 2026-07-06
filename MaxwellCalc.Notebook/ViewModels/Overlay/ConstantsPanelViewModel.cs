using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Parsers;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Core.Workspaces.Variables;
using System;

namespace MaxwellCalc.Notebook.ViewModels.Overlay;

/// <summary>
/// The command palette's Constants panel. Reads <c>workspace.Constants.Local</c>. The footer (Step 9)
/// adds a constant by name, value and optional description; every row can be removed with the × button.
/// </summary>
public sealed partial class ConstantsPanelViewModel : FilteredPanelViewModel<ConstantItem, string, Variable<string>>
{
    /// <summary>Gets or sets the footer's name field.</summary>
    [ObservableProperty]
    private string _newName = string.Empty;

    /// <summary>Gets or sets the footer's value field.</summary>
    [ObservableProperty]
    private string _newValue = string.Empty;

    /// <summary>Gets or sets the footer's optional description field.</summary>
    [ObservableProperty]
    private string _newDescription = string.Empty;

    /// <summary>Gets or sets the inline error shown when an add fails.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    /// <summary>Gets whether an inline error is showing.</summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Creates a new <see cref="ConstantsPanelViewModel"/>.
    /// </summary>
    /// <param name="workspaceState">The shared workspace state.</param>
    public ConstantsPanelViewModel(WorkspaceState workspaceState)
        : base(workspaceState)
    {
    }

    /// <inheritdoc />
    protected override IReadOnlyObservableDictionary<string, Variable<string>>? GetDictionary(IWorkspace workspace)
        => workspace.Constants.Local;

    /// <inheritdoc />
    protected override ConstantItem Project(string key, Variable<string> value)
        => new() { Name = key, Description = value.Description, Value = value.Value };

    /// <inheritdoc />
    protected override bool Matches(ConstantItem item, string filter)
        => item.Name.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || (item.Description?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false);

    /// <inheritdoc />
    protected override int Compare(ConstantItem a, ConstantItem b)
        => StringComparer.OrdinalIgnoreCase.Compare(a.Name, b.Name);

    /// <summary>Adds a constant from the footer fields, binding it into <c>workspace.Constants</c>.</summary>
    [RelayCommand]
    private void Add()
    {
        if (string.IsNullOrWhiteSpace(NewName) || string.IsNullOrWhiteSpace(NewValue))
            return;

        ErrorMessage = RunWithDiagnostics(workspace =>
        {
            string name = NewName.Trim();
            string? description = string.IsNullOrWhiteSpace(NewDescription) ? null : NewDescription.Trim();
            var node = Parser.Parse(new Lexer(NewValue), workspace);
            return node is not null && workspace.Constants.TryAssignVariable(name, node, description);
        });

        if (ErrorMessage is null)
        {
            NewName = string.Empty;
            NewValue = string.Empty;
            NewDescription = string.Empty;
        }
    }

    /// <summary>Removes a constant (the row × button).</summary>
    /// <param name="item">The row to remove.</param>
    [RelayCommand]
    private void Remove(ConstantItem item)
        => WorkspaceState.Workspace?.Constants.TryRemoveVariable(item.Name);
}
