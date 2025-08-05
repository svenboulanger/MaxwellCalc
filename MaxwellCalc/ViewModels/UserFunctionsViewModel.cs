using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
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
    public partial class UserFunctionsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private IWorkspace? _workspace;

        [ObservableProperty]
        private UserFunctionViewModel? _selectedItem;

        [ObservableProperty]
        private ObservableCollection<UserFunctionViewModel> _functions = [];

        [ObservableProperty]
        private ObservableCollection<UserFunctionViewModel> _filteredFunctions = [];

        [ObservableProperty]
        private string _filter = string.Empty;

        /// <summary>
        /// Creates a new <see cref="UserFunctionsViewModel"/>.
        /// </summary>
        public UserFunctionsViewModel()
        {
            if (Design.IsDesignMode)
            {
                for (int i = 0; i < 5; i++)
                {
                    var model = new UserFunctionViewModel() { Name = $"Test", Arguments = ["a", "b"], Value = "a + b" };
                    Functions.Add(model);
                    FilteredFunctions.Add(model);
                }
            }
        }

        /// <summary>
        /// Creates a new <see cref="UserFunctionsViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public UserFunctionsViewModel(IServiceProvider sp)
        {
            _workspace = sp.GetRequiredService<IWorkspace>();

            // Add all the functions
            foreach (var userFunction in _workspace.UserFunctions)
            {
                var model = new UserFunctionViewModel() { Name = userFunction.Name, Arguments = [.. userFunction.Parameters], Value = userFunction.Body };
                Functions.Add(model);
                FilteredFunctions.Add(model);
            }

            _workspace.FunctionChanged += OnWorkspaceFunctionChanged;
        }

        private bool MatchesFilter(UserFunctionViewModel model)
            => model.Name?.Contains(Filter, StringComparison.OrdinalIgnoreCase) ?? false;

        [RelayCommand]
        private void ApplyFilter()
        {
            FilteredFunctions.Clear();
            foreach (var item in Functions.Where(MatchesFilter))
                FilteredFunctions.Add(item);
        }

        [RelayCommand]
        private void RemoveItem(UserFunctionViewModel model)
        {
            Functions.Remove(model);
            FilteredFunctions.Remove(model);

            if (Workspace is not null && model.Name is not null && model.Arguments is not null)
                Workspace.TryRemoveUserFunction(model.Name, model.Arguments.Count);
        }

        partial void OnFilterChanged(string? oldValue, string newValue) => ApplyFilter();

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
            FilteredFunctions.Clear();
            foreach (var userFunction in newValue.UserFunctions)
            {
                var model = new UserFunctionViewModel() { Name = userFunction.Name, Arguments = [.. userFunction.Parameters], Value = userFunction.Body };
                Functions.Add(model);
                if (MatchesFilter(model))
                    FilteredFunctions.Add(model);
            }

            // Register for the new workspace
            if (newValue is not null)
                newValue.FunctionChanged += OnWorkspaceFunctionChanged;

            ApplyFilter();
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
                    Functions.Add(new UserFunctionViewModel()
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

            ApplyFilter();
        }
    }
}
