using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A view model for the function list.
    /// </summary>
    public partial class FunctionsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private IWorkspace? _workspace;

        [ObservableProperty]
        private VariableViewModel? _selectedItem;

        [ObservableProperty]
        private ObservableCollection<FunctionViewModel> _functions = [];

        /// <summary>
        /// Creates a new <see cref="FunctionsViewModel"/>.
        /// </summary>
        public FunctionsViewModel()
        {
            if (Design.IsDesignMode)
            {
                for (int i = 0; i < 5; i++)
                    _functions.Add(new() { Name = $"Test", Arguments = ["a", "b"], Value = "a + b" });
            }
        }

        /// <summary>
        /// Creates a new <see cref="FunctionsViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public FunctionsViewModel(IServiceProvider sp)
        {
            _workspace = sp.GetRequiredService<IWorkspace>();

            // Add all the functions
            foreach (var userFunction in _workspace.UserFunctions)
                Functions.Add(new() { Name = userFunction.Name, Arguments = [ .. userFunction.Parameters ], Value = userFunction.Body });

            _workspace.FunctionChanged += OnWorkspaceFunctionChanged;
            Functions.CollectionChanged += FunctionsChanged;
        }

        private void FunctionsChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Workspace is null)
                return; // No need to synchronize

            // Get the old and new values by name
            HashSet<(string, IEnumerable<string>?)> toRemove = [];
            Dictionary<(string, IEnumerable<string>?), string> toSet = [];
            if (e.OldItems is not null)
            {
                foreach (var item in e.OldItems.OfType<FunctionViewModel>())
                {
                    if (item.Name is null)
                        continue;
                    toRemove.Add((item.Name, item.Arguments));
                }
            }
            if (e.NewItems is not null)
            {
                foreach (var item in e.NewItems.OfType<FunctionViewModel>())
                {
                    if (item.Name is null)
                        continue;
                    toRemove.Remove((item.Name, item.Arguments));
                    toSet.Add((item.Name, item.Arguments), item.Value ?? string.Empty);
                }
            }

            // Now update
            foreach (var key in toRemove)
                Workspace.TryRemoveUserFunction(key.Item1, key.Item2?.Count() ?? 0);
            foreach (var pair in toSet)
                Workspace.TryRegisterUserFunction(new UserFunction(pair.Key.Item1, pair.Key.Item2?.ToArray() ?? [], pair.Value));
        }

        partial void OnWorkspaceChanged(IWorkspace? oldValue, IWorkspace? newValue)
        {
            if (oldValue is not null)
                oldValue.FunctionChanged -= OnWorkspaceFunctionChanged;
            if (newValue is null)
                return; // No need to do anything

            // Let's try to bring all our user functions to the new workspace
            foreach (var functionModel in Functions)
            {
                string name = functionModel.Name ?? string.Empty;
                newValue.TryRegisterUserFunction(new UserFunction(name, [.. functionModel.Arguments ?? []], functionModel.Value ?? string.Empty));
            }

            // Rebuild our internal list of functions
            Functions.Clear();
            foreach (var userFunction in newValue.UserFunctions)
                Functions.Add(new() { Name = userFunction.Name, Arguments = [.. userFunction.Parameters], Value = userFunction.Body });

            // Register for the new workspace
            if (newValue is not null)
                newValue.FunctionChanged += OnWorkspaceFunctionChanged;
        }

        private void OnWorkspaceFunctionChanged(object? sender, FunctionChangedEvent e)
        {
            if (e.Name is null || Workspace is null)
                return;

            // Check if we have a model for it
            var model = Functions.FirstOrDefault(item => item?.Name?.Equals(e.Name) ?? false && item.Arguments.Count == e.Arguments);
            if (Workspace.TryGetUserFunction(e.Name, e.Arguments, out var function))
            {
                if (model is not null)
                {
                    // This is a change to an existing function
                    model.Name = e.Name;
                    model.Arguments = [.. function.Parameters];
                    model.Value = function.Body;
                }
                else
                {
                    // This is a new one
                    Functions.Add(new FunctionViewModel()
                    {
                        Name = e.Name,
                        Arguments = [.. function.Parameters],
                        Value = function.Body
                    });
                }
            }
            else
            {
                // This is a removed one
                if (model is not null)
                    Functions.Remove(model);
            }
        }
    }
}
