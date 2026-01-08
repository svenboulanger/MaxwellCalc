using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MaxwellCalc.Core.Domains;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Numerics;
using System.Text.Json;

namespace MaxwellCalc;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();
        services.AddSingleton<CalculatorViewModel>();

        services.AddSingleton<VariablesViewModel>();
        services.AddSingleton<UserVariablesViewModel>();
        services.AddSingleton<ConstantsViewModel>();

        services.AddSingleton<FunctionsViewModel>();
        services.AddSingleton<UserFunctionsViewModel>();
        services.AddSingleton<BuiltInFunctionsViewModel>();

        services.AddSingleton<UnitsViewModel>();
        services.AddSingleton<InputUnitsViewModel>();
        services.AddSingleton<OutputUnitsViewModel>();

        services.AddSingleton<SharedModel>();
        services.AddSingleton<SettingsViewModel>();

        // Workspaces
        var jsonSerializerOptions = new JsonSerializerOptions() { WriteIndented = true };
        jsonSerializerOptions.Converters.Add(new WorkspaceJsonConverter()); // This will handle IWorkspace conversion
        jsonSerializerOptions.Converters.Add(new WorkspaceJsonConverterFactory()); // This will handle IWorkspace<T> conversion.
        services.AddSingleton(jsonSerializerOptions);

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
}