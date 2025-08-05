using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MaxwellCalc.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A view model for the built-in function list.
    /// </summary>
    public partial class BuiltInFunctionsViewModel : ViewModelBase
    {
        [ObservableProperty]
        private IWorkspace? _workspace;

        [ObservableProperty]
        private BuiltInFunctionViewModel? _selectedItem;

        [ObservableProperty]
        private ObservableCollection<BuiltInFunctionViewModel> _functions = [];

        [ObservableProperty]
        private ObservableCollection<BuiltInFunctionViewModel> _filteredFunctions = [];

        [ObservableProperty]
        private string _filter = string.Empty;

        /// <summary>
        /// Creates a new <see cref="BuiltInFunctionViewModel"/>.
        /// </summary>
        public BuiltInFunctionsViewModel()
        {
            if (Design.IsDesignMode)
            {
                _functions.Add(new BuiltInFunctionViewModel
                {
                    Name = "sin",
                    MinArgCount = 1,
                    MaxArgCount = 1,
                    Description = "Calculates the sine of a real number"
                });
                _functions.Add(new BuiltInFunctionViewModel
                {
                    Name = "cos",
                    MinArgCount = 1,
                    MaxArgCount = 1,
                    Description = "Calculates the cosine of a real number"
                });
                _functions.Add(new BuiltInFunctionViewModel
                {
                    Name = "tan",
                    MinArgCount = 1,
                    MaxArgCount = 1,
                    Description = "Calculates the tangent of a real number"
                });
                _functions.Add(new BuiltInFunctionViewModel
                {
                    Name = "min",
                    MinArgCount = 1,
                    MaxArgCount = int.MaxValue,
                    Description = "Calculates the min of a real number"
                });
                _functions.Add(new BuiltInFunctionViewModel
                {
                    Name = "round",
                    MinArgCount = 1,
                    MaxArgCount = 2,
                    Description = "Rounds a number to some precision. If the precision is not given, then it will round to 0 digits after the comma."
                });
            }
        }

        /// <summary>
        /// Creates a new <see cref="BuiltInFunctionsViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public BuiltInFunctionsViewModel(IServiceProvider sp)
        {
            _workspace = sp.GetRequiredService<IWorkspace>();

            // Add all the built-in functions
            foreach (var builtInFunction in _workspace.BuiltInFunctions)
            {
                var model = new BuiltInFunctionViewModel()
                {
                    Name = builtInFunction.Name,
                    MinArgCount = builtInFunction.MinimumArgumentCount,
                    MaxArgCount = builtInFunction.MaximumArgumentCount,
                    Description = builtInFunction.Description
                };
                Functions.Add(model);
                FilteredFunctions.Add(model);
            }
        }

        private bool MatchesFilter(BuiltInFunctionViewModel model)
            => (model.Name?.Contains(Filter, StringComparison.OrdinalIgnoreCase) ?? false) ||
               (model.Description?.Contains(Filter, StringComparison.OrdinalIgnoreCase) ?? false);

        [RelayCommand]
        private void ApplyFilter()
        {
            FilteredFunctions.Clear();
            foreach (var item in Functions.Where(MatchesFilter))
                FilteredFunctions.Add(item);
        }

        [RelayCommand]
        private void RemoveItem(BuiltInFunctionViewModel model)
        {
            Functions.Remove(model);
        }

        partial void OnFilterChanged(string? oldValue, string newValue)
        {
            ApplyFilter();
        }

        partial void OnWorkspaceChanged(IWorkspace? oldValue, IWorkspace? newValue)
        {
            if (oldValue is not null)
                Functions.Clear();
            if (newValue is not null)
            {
                // Add all the built-in functions
                foreach (var builtInFunction in newValue.BuiltInFunctions)
                    Functions.Add(new()
                    {
                        Name = builtInFunction.Name,
                        MinArgCount = builtInFunction.MinimumArgumentCount,
                        MaxArgCount = builtInFunction.MaximumArgumentCount,
                        Description = builtInFunction.Description
                    });
            }
            ApplyFilter();
        }
    }
}
