using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;

namespace MaxwellCalc.ViewModels
{
    public partial class VariablesViewModel : ViewModelBase
    {
        private readonly UserVariablesViewModel _userVariables;
        private readonly ConstantsViewModel _constantVariables;

        [ObservableProperty]
        public IWorkspace? _workspace;

        [ObservableProperty]
        private int _selectedListItem;

        [ObservableProperty]
        private ViewModelBase _currentPage;

        /// <summary>
        /// Creates a new <see cref="FunctionsViewModel"/>.
        /// </summary>
        public VariablesViewModel()
        {
            _userVariables = new UserVariablesViewModel();
            _constantVariables = new ConstantsViewModel();
            _currentPage = _userVariables;

            _userVariables.PropertyChanged += UserVariablesPropertyChanged;
            _constantVariables.PropertyChanging += ConstantsPropertyChanged;
        }

        /// <summary>
        /// Creates a new <see cref="FunctionsViewModel"/>.
        /// </summary>
        /// <param name="sp"></param>
        public VariablesViewModel(IServiceProvider sp)
        {
            _userVariables = sp.GetRequiredService<UserVariablesViewModel>();
            _constantVariables = sp.GetRequiredService<ConstantsViewModel>();
            _currentPage = _userVariables;

            _userVariables.PropertyChanged += UserVariablesPropertyChanged;
            _constantVariables.PropertyChanging += ConstantsPropertyChanged;
        }

        partial void OnSelectedListItemChanged(int value)
        {
            switch (value)
            {
                case 0: CurrentPage = _userVariables; break;
                case 1: CurrentPage = _constantVariables; break;
            }
        }

        partial void OnWorkspaceChanged(IWorkspace? oldValue, IWorkspace? newValue)
        {
            if (ReferenceEquals(oldValue, newValue))
                return;
            _userVariables.Workspace = newValue;
            _userVariables.Workspace = newValue;
        }

        private void ConstantsPropertyChanged(object? sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName != nameof(Workspace))
                return;
            Workspace = _constantVariables.Workspace;
        }

        private void UserVariablesPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Workspace))
                return;
            Workspace = _userVariables.Workspace;
        }
    }
}
