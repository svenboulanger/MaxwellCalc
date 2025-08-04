using Avalonia.OpenGL.Surfaces;
using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.ComponentModel;

namespace MaxwellCalc.ViewModels
{
    public partial class FunctionsViewModel : ViewModelBase
    {
        private readonly UserFunctionsViewModel _userFunctions;
        private readonly BuiltInFunctionsViewModel _builtInFunctions;

        [ObservableProperty]
        public IWorkspace? _workspace;

        [ObservableProperty]
        private int _selectedListItem;

        [ObservableProperty]
        private ViewModelBase _currentPage;

        /// <summary>
        /// Creates a new <see cref="FunctionsViewModel"/>.
        /// </summary>
        public FunctionsViewModel()
        {
            _userFunctions = new UserFunctionsViewModel();
            _builtInFunctions = new BuiltInFunctionsViewModel();
            _currentPage = _userFunctions;

            _userFunctions.PropertyChanged += UserFunctionsPropertyChanged;
            _builtInFunctions.PropertyChanging += BuiltInFunctionsPropertyChanged;
        }

        /// <summary>
        /// Creates a new <see cref="FunctionsViewModel"/>.
        /// </summary>
        /// <param name="sp"></param>
        public FunctionsViewModel(IServiceProvider sp)
        {
            _userFunctions = sp.GetRequiredService<UserFunctionsViewModel>();
            _builtInFunctions = sp.GetRequiredService<BuiltInFunctionsViewModel>();
            _currentPage = _userFunctions;

            _userFunctions.PropertyChanged += UserFunctionsPropertyChanged;
            _builtInFunctions.PropertyChanging += BuiltInFunctionsPropertyChanged;
        }

        partial void OnSelectedListItemChanged(int value)
        {
            switch (value)
            {
                case 0: CurrentPage = _userFunctions; break;
                case 1: CurrentPage = _builtInFunctions; break;
            }
        }

        partial void OnWorkspaceChanged(IWorkspace? oldValue, IWorkspace? newValue)
        {
            if (ReferenceEquals(oldValue, newValue))
                return;
            _userFunctions.Workspace = newValue;
            _userFunctions.Workspace = newValue;
        }

        private void BuiltInFunctionsPropertyChanged(object? sender, PropertyChangingEventArgs e)
        {
            if (e.PropertyName != nameof(Workspace))
                return;
            Workspace = _builtInFunctions.Workspace;
        }

        private void UserFunctionsPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(Workspace))
                return;
            Workspace = _userFunctions.Workspace;
        }
    }
}
