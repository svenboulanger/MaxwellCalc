using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MaxwellCalc.ViewModels
{
    public partial class VariablesViewModel : ViewModelBase
    {
        private readonly UserVariablesViewModel _userVariables;
        private readonly ConstantsViewModel _constantVariables;

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
        }

        partial void OnSelectedListItemChanged(int value)
        {
            switch (value)
            {
                case 0: CurrentPage = _userVariables; break;
                case 1: CurrentPage = _constantVariables; break;
            }
        }
    }
}
