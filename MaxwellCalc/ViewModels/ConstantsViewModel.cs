using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MaxwellCalc.ViewModels
{
    public partial class ConstantsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private IWorkspace? _workspace;

        [ObservableProperty]
        private VariableViewModel? _selectedItem;

        [ObservableProperty]
        ObservableCollection<VariableViewModel> _constants = [];

        [ObservableProperty]
        ObservableCollection<VariableViewModel> _filteredConstants = [];

        [ObservableProperty]
        private string _filter = string.Empty;

        /// <summary>
        /// Creates a new <see cref="ConstantsViewModel"/>.
        /// </summary>
        public ConstantsViewModel()
        {
            if (Design.IsDesignMode)
            {
                // Make up some constants to show what it looks like
                InsertModel(new()
                {
                    Name = "Sven",
                    Value = new Quantity<string>("179", new Unit(("cm", 1))),
                    Description = "The length of the author."
                });
                InsertModel(new()
                {
                    Name = "CheckThis",
                    Value = new Quantity<string>("123", Unit.UnitAmperes)
                });
            }
        }

        /// <summary>
        /// Creates a new <see cref="ConstantsViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public ConstantsViewModel(IServiceProvider sp)
        {
            _workspace = sp.GetRequiredService<IWorkspace>();

            // Add all the constants
            foreach (var variable in _workspace.Constants)
                InsertModel(new VariableViewModel() { Name = variable.Name, Value = variable.Value, Description = variable.Description });
            _workspace.VariableChanged += OnWorkspaceVariableChanged;
        }

        private bool MatchesFilter(VariableViewModel model)
            => string.IsNullOrWhiteSpace(Filter) ||
            (model.Name?.Contains(Filter, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (model.Description?.Contains(Filter, StringComparison.OrdinalIgnoreCase) ?? false);

        private void ApplyFilter()
        {
            FilteredConstants.Clear();
            foreach (var model in Constants.Where(MatchesFilter))
                FilteredConstants.Add(model);
        }

        partial void OnFilterChanged(string value) => ApplyFilter();

        [RelayCommand]
        private void RemoveItem(VariableViewModel model)
        {
            Constants.Remove(model);
            FilteredConstants.Remove(model);
            if (Workspace is not null && model.Name is not null)
                Workspace.TryRemoveVariable(model.Name);
        }

        partial void OnWorkspaceChanged(IWorkspace? oldValue, IWorkspace? newValue)
        {
            // Unregister from the last workspace
            if (oldValue is not null)
                oldValue.VariableChanged -= OnWorkspaceVariableChanged;
            if (newValue is null)
                return; // No need to do anything

            // Let's try to bring all our constants to the new workspace
            foreach (var variableModel in Constants)
            {
                string name = variableModel.Name ?? string.Empty;
                newValue.TrySetVariable(new Variable(name, variableModel.Value, variableModel.Description));
            }

            // Rebuild our internal list of constants
            Constants.Clear();
            FilteredConstants.Clear();
            foreach (var variable in newValue.Constants)
                InsertModel(new VariableViewModel() { Name = variable.Name, Value = variable.Value, Description = variable.Description });

            // Register for the new workspace
            if (newValue is not null)
                newValue.VariableChanged += OnWorkspaceVariableChanged;
        }

        private void OnWorkspaceVariableChanged(object? sender, VariableChangedEvent args)
        {
            // Let's update our own list with this
            // First let's find the model that reflects this variable
            if (args.Name is null || Workspace is null)
                return;

            // Check if we have a model for it
            var model = Constants.FirstOrDefault(item => item?.Name?.Equals(args.Name) ?? false);

            if (Workspace.TryGetConstant(args.Name, out var value, out string? description))
            {
                if (model is not null)
                {
                    // This is an existing variable update
                    model.Value = value;
                }
                else
                {
                    // This is an unknown variable for now
                    InsertModel(new() { Name = args.Name, Value = value, Description = description });
                }
            }
            else
            {
                // This is a removed variable
                if (model is not null)
                {
                    Constants.Remove(model);
                    FilteredConstants.Remove(model);
                }
            }
        }

        private void InsertModel(VariableViewModel model)
        {
            if (model.Name is null)
                throw new ArgumentNullException(nameof(model.Name));

            // Insert into the main list
            int index = 0;
            while (index < Constants.Count &&
                StringComparer.Ordinal.Compare(model.Name, Constants[index].Name ?? string.Empty) > 0)
                index++;
            Constants.Insert(index, model);

            // Insert into the filtered list
            if (MatchesFilter(model))
            {
                index = 0;
                while (index < FilteredConstants.Count &&
                    StringComparer.Ordinal.Compare(model.Name, FilteredConstants[index].Name) > 0)
                    index++;
                FilteredConstants.Insert(index, model);
            }
        }
    }
}
