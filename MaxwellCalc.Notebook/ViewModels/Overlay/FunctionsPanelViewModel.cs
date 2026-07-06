using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Parsers;
using MaxwellCalc.Core.Parsers.Nodes;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaxwellCalc.Notebook.ViewModels.Overlay;

/// <summary>
/// The command palette's Functions panel: the workspace's user-defined functions plus the read-only
/// <c>from sheet</c> functions defined on the sheet this session. Reads <c>workspace.UserFunctions</c>.
/// The footer (Step 9) defines a function from its signature and body; rows remove a user function.
/// </summary>
public sealed partial class FunctionsPanelViewModel : FilteredPanelViewModel<FunctionItem, UserFunctionKey, UserFunction>
{
    private readonly SheetViewModel _sheet;

    /// <summary>Gets or sets the footer's signature field (<c>f(x)</c>).</summary>
    [ObservableProperty]
    private string _newSignature = string.Empty;

    /// <summary>Gets or sets the footer's body field.</summary>
    [ObservableProperty]
    private string _newBody = string.Empty;

    /// <summary>Gets or sets the inline error shown when an add fails.</summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? _errorMessage;

    /// <summary>Gets whether an inline error is showing.</summary>
    public bool HasError => !string.IsNullOrEmpty(ErrorMessage);

    /// <summary>
    /// Creates a new <see cref="FunctionsPanelViewModel"/>.
    /// </summary>
    /// <param name="workspaceState">The shared workspace state.</param>
    /// <param name="sheet">The sheet, for the <c>from sheet</c> functions.</param>
    public FunctionsPanelViewModel(WorkspaceState workspaceState, SheetViewModel sheet)
        : base(workspaceState)
    {
        _sheet = sheet;
        _sheet.SheetSymbolsChanged += ScheduleRebuild;
        ScheduleRebuild();
    }

    /// <inheritdoc />
    protected override IReadOnlyObservableDictionary<UserFunctionKey, UserFunction>? GetDictionary(IWorkspace workspace)
        => workspace.UserFunctions.AsReadOnly();

    /// <inheritdoc />
    protected override FunctionItem Project(UserFunctionKey key, UserFunction value)
        => new()
        {
            Key = key,
            Signature = $"{key.Name}({string.Join(", ", value.Parameters)})",
            Body = string.Join("; ", value.Body.Select(node => node.Content.ToString())),
            CanRemove = true,
            FromSheet = false,
        };

    /// <summary>
    /// Defines a function from the footer fields: the signature (parsed to a function node whose
    /// arguments must be simple variables) and the body, registered into <c>workspace.UserFunctions</c>,
    /// mirroring the old <c>UserFunctionsViewModel.AddUserFunction</c> flow.
    /// </summary>
    [RelayCommand]
    private void Add()
    {
        if (string.IsNullOrWhiteSpace(NewSignature) || string.IsNullOrWhiteSpace(NewBody))
            return;

        ErrorMessage = RunWithDiagnostics(workspace =>
        {
            if (Parser.Parse(new Lexer(NewSignature), workspace) is not FunctionNode signature)
            {
                workspace.PostDiagnosticMessage(new("The signature must look like f(x)."));
                return false;
            }

            var parameters = new string[signature.Arguments.Count];
            for (int i = 0; i < signature.Arguments.Count; i++)
            {
                if (signature.Arguments[i] is not VariableNode argument)
                {
                    workspace.PostDiagnosticMessage(new("A function argument has to be a simple variable."));
                    return false;
                }
                parameters[i] = argument.Content.ToString();
            }

            var body = new List<INode>();
            foreach (var line in NewBody.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
            {
                var node = Parser.Parse(new Lexer(line), workspace);
                if (node is null)
                    return false;
                body.Add(node);
            }
            if (body.Count == 0)
                return false;

            workspace.UserFunctions[new UserFunctionKey(signature.Name, parameters.Length)] =
                new UserFunction(parameters, [.. body]);
            return true;
        });

        if (ErrorMessage is null)
        {
            NewSignature = string.Empty;
            NewBody = string.Empty;
        }
    }

    /// <summary>Removes a user function (the row × button). <c>from sheet</c> rows have no button.</summary>
    /// <param name="item">The row to remove.</param>
    [RelayCommand]
    private void Remove(FunctionItem item)
    {
        if (item.Key is { } key)
            WorkspaceState.Workspace?.UserFunctions.Remove(key);
    }

    /// <inheritdoc />
    protected override IEnumerable<FunctionItem> ExtraItems()
        // Null-safe: the base constructor's first Rebuild runs before this derived constructor assigns
        // _sheet (see VariablesPanelViewModel for the same pattern).
        => (_sheet?.SheetFunctions ?? []).Select(signature => new FunctionItem
        {
            Signature = signature,
            Body = null,
            CanRemove = false,
            FromSheet = true,
        });

    /// <inheritdoc />
    protected override bool Matches(FunctionItem item, string filter)
        => item.Signature.Contains(filter, StringComparison.OrdinalIgnoreCase)
        || (item.Body?.Contains(filter, StringComparison.OrdinalIgnoreCase) ?? false);

    /// <inheritdoc />
    protected override int Compare(FunctionItem a, FunctionItem b)
    {
        // Workspace functions first, sheet functions after; alphabetical within each group.
        if (a.FromSheet != b.FromSheet)
            return a.FromSheet ? 1 : -1;
        return StringComparer.OrdinalIgnoreCase.Compare(a.Signature, b.Signature);
    }
}
