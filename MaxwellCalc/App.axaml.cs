using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MaxwellCalc.Domains;
using MaxwellCalc.Units;
using MaxwellCalc.ViewModels;
using MaxwellCalc.Workspaces;
using Microsoft.Extensions.DependencyInjection;

namespace MaxwellCalc
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            var services = new ServiceCollection();
            services.AddSingleton(sp => new CalculatorViewModel(sp));
            services.AddSingleton(sp => new VariablesViewModel(sp));
            services.AddSingleton(sp => new FunctionsViewModel(sp));
            services.AddSingleton(sp => new SettingsViewModel(sp));
            services.AddSingleton(BuildDefaultWorkspace());

            var serviceProvider = services.BuildServiceProvider();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow()
                {
                    DataContext = new MainWindowViewModel(serviceProvider)
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

        private IWorkspace BuildDefaultWorkspace()
        {
            var workspace = new Workspace<double>(new DoubleDomain());
            DoubleMathHelper.RegisterCommonConstants(workspace);
            DoubleMathHelper.RegisterCommonElectronicsConstants(workspace);
            DoubleMathHelper.RegisterFunctions(workspace);
            UnitHelper.RegisterCommonUnits(workspace);
            UnitHelper.RegisterCommonElectronicsUnits(workspace);
            return workspace;
        }
    }
}