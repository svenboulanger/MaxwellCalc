using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MaxwellCalc.ViewModels
{
    public partial class VariablesViewModel : ViewModelBase
    {
        [ObservableProperty]
        private IWorkspace? _workspace;

        [ObservableProperty]
        private VariableViewModel? _selectedItem;

        [ObservableProperty]
        ObservableCollection<VariableViewModel> _variables = [];

        /// <summary>
        /// Creates a new <see cref="VariablesViewModel"/>.
        /// </summary>
        public VariablesViewModel()
        {
            if (Design.IsDesignMode)
            {
                // Make up some variables to show what it looks like
                _variables = [
                    new VariableViewModel() {
                        Name = "Sven",
                        Value = new Quantity<string>("179", new Unit(("cm", 1)))
                    },
                    new VariableViewModel() {
                        Name = "Elke",
                        Value = new Quantity<string>("150", new Unit(("cm", 1)))
                    }
                ];
            }
        }

        /// <summary>
        /// Creates a new <see cref="VariablesViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public VariablesViewModel(IServiceProvider sp)
        {
            _workspace = sp.GetRequiredService<IWorkspace>();

            // Add all the variables
            foreach (var variable in _workspace.Variables)
                Variables.Add(new() { Name = variable.Name, Value = variable.Value });

            _workspace.VariableChanged += OnWorkspaceVariableChanged;
            Variables.CollectionChanged += VariablesChanged;
        }

        private void VariablesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Workspace is null)
                return; // No need to synchronize

            // Get the old and new values by the name
            HashSet<string> toRemove = [];
            Dictionary<string, Quantity<string>> toSet = [];
            if (e.OldItems is not null)
            {
                foreach (var item in e.OldItems.OfType<VariableViewModel>())
                {
                    if (item.Name is null)
                        continue;
                    toRemove.Add(item.Name);
                }
            }
            if (e.NewItems is not null)
            {
                foreach (var item in e.NewItems.OfType<VariableViewModel>())
                {
                    if (item.Name is null)
                        continue;
                    toRemove.Remove(item.Name);
                    toSet.Add(item.Name, item.Value);
                }
            }

            // Now we need to update the workspace to reflect these changes
            foreach (string name in toRemove)
                Workspace.TryRemoveVariable(name);
            foreach (var pair in toSet)
                Workspace.TrySetVariable(new(pair.Key, pair.Value));
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
                newValue.TrySetVariable(new Variable(name, variableModel.Value));
            }

            // Rebuild our internal list of variables
            Variables.Clear();
            foreach (var variable in newValue.Variables)
                Variables.Add(new() { Name = variable.Name, Value = variable.Value });

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
                    Variables.Add(new() { Name = args.Name, Value = value });
                }
            }
            else
            {
                // This is a removed variable
                if (model is not null)
                    Variables.Remove(model);
            }
        }
    }
}
