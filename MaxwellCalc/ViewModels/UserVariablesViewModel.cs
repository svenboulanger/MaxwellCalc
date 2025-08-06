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
    public partial class UserVariablesViewModel : ViewModelBase
    {
        [ObservableProperty]
        private IWorkspace? _workspace;

        [ObservableProperty]
        private VariableViewModel? _selectedItem;

        [ObservableProperty]
        ObservableCollection<VariableViewModel> _variables = [];

        [ObservableProperty]
        ObservableCollection<VariableViewModel> _filteredVariables = [];

        [ObservableProperty]
        private string _filter = string.Empty;

        /// <summary>
        /// Creates a new <see cref="UserVariablesViewModel"/>.
        /// </summary>
        public UserVariablesViewModel()
        {
            if (Design.IsDesignMode)
            {
                // Make up some variables to show what it looks like
                InsertModel(new VariableViewModel()
                {
                    Name = "Sven",
                    Value = new Quantity<string>("179", new Unit(("cm", 1)))
                });
                InsertModel(new VariableViewModel()
                {
                    Name = "Elke",
                    Value = new Quantity<string>("150", new Unit(("cm", 1)))
                });
            }
        }

        /// <summary>
        /// Creates a new <see cref="UserVariablesViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public UserVariablesViewModel(IServiceProvider sp)
        {
            _workspace = sp.GetRequiredService<IWorkspace>();

            // Add all the variables
            foreach (var variable in _workspace.Variables)
                InsertModel(new VariableViewModel() { Name = variable.Name, Value = variable.Value });

            _workspace.VariableChanged += OnWorkspaceVariableChanged;
        }

        private bool MatchesFilter(VariableViewModel model)
            => string.IsNullOrWhiteSpace(Filter) ||
            (model.Name?.Contains(Filter, StringComparison.OrdinalIgnoreCase) ?? false);

        private void ApplyFilter()
        {
            FilteredVariables.Clear();
            foreach (var model in Variables.Where(MatchesFilter))
                FilteredVariables.Add(model);
        }

        partial void OnFilterChanged(string value) => ApplyFilter();

        [RelayCommand]
        private void RemoveItem(VariableViewModel model)
        {
            Variables.Remove(model);
            FilteredVariables.Remove(model);
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

            // Let's try to bring all our variables to the new workspace
            foreach (var variableModel in Variables)
            {
                string name = variableModel.Name ?? string.Empty;
                newValue.TrySetVariable(new Variable(name, variableModel.Value, variableModel.Description));
            }

            // Rebuild our internal list of variables
            Variables.Clear();
            FilteredVariables.Clear();
            foreach (var variable in newValue.Variables)
                InsertModel(new VariableViewModel() { Name = variable.Name, Value = variable.Value });

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
            var model = Variables.FirstOrDefault(item => item?.Name?.Equals(args.Name) ?? false);

            if (Workspace.TryGetVariable(args.Name, out var value))
            {
                if (model is not null)
                {
                    // This is an existing variable update
                    model.Value = value;
                }
                else
                {
                    // This is an unknown variable for now
                    InsertModel(new VariableViewModel() { Name = args.Name, Value = value });
                }
            }
            else
            {
                // This is a removed variable
                if (model is not null)
                {
                    Variables.Remove(model);
                    FilteredVariables.Remove(model);
                }
            }
        }

        private void InsertModel(VariableViewModel model)
        {
            if (model.Name is null)
                throw new ArgumentNullException(nameof(model.Name));

            // Insert into the main list
            int index = 0;
            while (index < Variables.Count &&
                StringComparer.Ordinal.Compare(model.Name, Variables[index].Name ?? string.Empty) > 0)
                index++;
            Variables.Insert(index, model);

            // Insert into the filtered list
            if (MatchesFilter(model))
            {
                index = 0;
                while (index < FilteredVariables.Count &&
                    StringComparer.Ordinal.Compare(model.Name, FilteredVariables[index].Name) > 0)
                    index++;
                FilteredVariables.Insert(index, model);
            }
        }
    }
}
