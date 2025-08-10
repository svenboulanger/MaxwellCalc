using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace MaxwellCalc.ViewModels
{
    public partial class UnitsViewModel : ViewModelBase
    {
        private readonly InputUnitsViewModel _inputUnitsViewModel;
        private readonly OutputUnitsViewModel _outputUnitsViewModel;

        [ObservableProperty]
        private int _selectedListItem;

        [ObservableProperty]
        private ViewModelBase _currentPage;

        /// <summary>
        /// Creates a new <see cref="UnitsViewModel"/>.
        /// </summary>
        public UnitsViewModel()
        {
            _inputUnitsViewModel = new();
            _outputUnitsViewModel = new();
            _currentPage = _inputUnitsViewModel;
        }

        /// <summary>
        /// Creates a new <see cref="UnitsViewModel"/>.
        /// </summary>
        /// <param name="sp">The service provider.</param>
        public UnitsViewModel(IServiceProvider sp)
        {
            _inputUnitsViewModel = sp.GetRequiredService<InputUnitsViewModel>();
            _outputUnitsViewModel = sp.GetRequiredService<OutputUnitsViewModel>();
            _currentPage = _inputUnitsViewModel;
        }

        partial void OnSelectedListItemChanged(int value)
        {
            switch (value)
            {
                case 0: CurrentPage = _inputUnitsViewModel; break;
                case 1: CurrentPage = _outputUnitsViewModel; break;
            }
        }
    }
}
