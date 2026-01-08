using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Core.Dictionaries;
using MaxwellCalc.Core.Parsers;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using System;
using System.Collections.ObjectModel;
using System.Numerics;

namespace MaxwellCalc.ViewModels;

public partial class OutputUnitsViewModel : FilteredCollectionViewModel<OutputUnitViewModel, OutputUnitKey, string>
{
    [ObservableProperty]
    private string _unit = string.Empty;

    [ObservableProperty]
    private string _expression = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _diagnostics = [];

    /// <summary>
    /// Creates a new <see cref="OutputUnitsViewModel"/>.
    /// </summary>
    public OutputUnitsViewModel()
        : base()
    {
        if (Design.IsDesignMode)
        {
            if (Shared.Workspace?.Key is IWorkspace<double> workspace)
            {
                workspace.RegisterCommonUnits();
            }
        }
    }

    /// <summary>
    /// Creates a new <see cref="OutputUnitsViewModel"/>.
    /// </summary>
    /// <param name="sp">The service provider.</param>
    public OutputUnitsViewModel(IServiceProvider sp)
        : base(sp)
    {
    }

    /// <inheritdoc />
    protected override bool MatchesFilter(OutputUnitViewModel model)
    {
        if (string.IsNullOrWhiteSpace(Filter))
            return true;
        if (model.Unit.ToString().Contains(Filter, StringComparison.OrdinalIgnoreCase))
            return true;
        if (model.Value.ToString().Contains(Filter, StringComparison.OrdinalIgnoreCase))
            return true;
        return false;
    }

    /// <inheritdoc />
    protected override int CompareModels(OutputUnitViewModel a, OutputUnitViewModel b)
    {
        int c = a.Unit.ToString().CompareTo(b.Unit.ToString());
        if (c == 0)
            return a.Value.Unit.ToString().CompareTo(b.Value.Unit.ToString());
        return c;
    }

    /// <inheritdoc />
    protected override IReadOnlyObservableDictionary<OutputUnitKey, string> GetCollection(IWorkspace workspace)
        => workspace.OutputUnits;

    /// <inheritdoc />
    protected override void UpdateModel(OutputUnitViewModel model, OutputUnitKey key, string value)
    {
        model.Unit = key.OutputUnit;
        model.Value = new Quantity<string>(value, key.BaseUnit);
    }

    /// <inheritdoc />
    protected override void RemoveItem(OutputUnitKey key)
    {
        if (Shared.Workspace.Key is null)
            return;
        Shared.Workspace.Key.TryRemoveOutputUnit(key);
    }

    [RelayCommand]
    private void AddOutputUnit()
    {
        if (Shared.Workspace.Key is null || string.IsNullOrWhiteSpace(Unit) || string.IsNullOrWhiteSpace(Expression))
            return;

        // Deal with diagnostic messages
        Diagnostics.Clear();
        void AddDiagnosticMessage(object? sender, DiagnosticMessagePostedEventArgs args)
            => Diagnostics.Add(args.Message);
        Shared.Workspace.Key.DiagnosticMessagePosted += AddDiagnosticMessage;

        var oldState = Shared.Workspace.Key.Restrict(false, false, true, false, false);
        try
        {
            // Try to evaluate the output unit
            var lexer = new Lexer(Unit);
            var outputUnitsNode = Parser.Parse(lexer, Shared.Workspace.Key);
            if (outputUnitsNode is null)
                return;

            // Try to evaluate the expression to get to the input units
            lexer = new Lexer(Expression);
            var baseUnitsNode = Parser.Parse(lexer, Shared.Workspace.Key);
            if (baseUnitsNode is null)
                return;

            if (Shared.Workspace.Key.TryAssignOutputUnit(outputUnitsNode, baseUnitsNode))
            {
                // Reset
                Unit = string.Empty;
                Expression = string.Empty;
            }
        }
        finally
        {
            Shared.Workspace.Key.Restore(oldState);
            Shared.Workspace.Key.DiagnosticMessagePosted -= AddDiagnosticMessage;
        }
    }


    [RelayCommand]
    private void AddCommonUnits()
    {
        if (Shared.Workspace.Key is null)
            return;
        switch (Shared.Workspace.Key)
        {
            case IWorkspace<double> dblWorkspace:
                dblWorkspace.RegisterCommonUnits();
                break;

            case IWorkspace<Complex> cplxWorkspace:
                cplxWorkspace.RegisterCommonUnits();
                break;
        }
    }

    [RelayCommand]
    private void AddCommonElectronicsUnits()
    {
        if (Shared.Workspace.Key is null)
            return;
        switch (Shared.Workspace.Key)
        {
            case IWorkspace<double> dblWorkspace:
                UnitHelper.RegisterCommonElectronicsUnits(dblWorkspace);
                break;

            case IWorkspace<Complex> cplxWorkspace:
                UnitHelper.RegisterCommonElectronicsUnits(cplxWorkspace);
                break;
        }
    }
}
