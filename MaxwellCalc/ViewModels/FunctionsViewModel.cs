using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MaxwellCalc.ViewModels
{
    public partial class FunctionsViewModel : ViewModelBase
    {
        private readonly UserFunctionsViewModel _userFunctions;
        private readonly BuiltInFunctionsViewModel _builtInFunctions;

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
        }

        partial void OnSelectedListItemChanged(int value)
        {
            switch (value)
            {
                case 0: CurrentPage = _userFunctions; break;
                case 1: CurrentPage = _builtInFunctions; break;
            }
        }
    }
}
