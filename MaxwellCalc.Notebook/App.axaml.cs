using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Notebook.ViewModels;
using MaxwellCalc.Notebook.ViewModels.Overlay;
using MaxwellCalc.Notebook.Views;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

namespace MaxwellCalc.Notebook;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = new ServiceCollection();

        // JSON options for persisting workspaces (Core converters).
        var jsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
        jsonSerializerOptions.Converters.Add(new WorkspaceJsonConverter());
        jsonSerializerOptions.Converters.Add(new WorkspaceJsonConverterFactory());
        services.AddSingleton(jsonSerializerOptions);

        // Shared state + view models (all authored fresh; expanded in later steps).
        services.AddSingleton<WorkspaceState>();
        services.AddSingleton<SheetViewModel>();

        // Command-palette overlay panels (Step 8), each driving a Core dictionary directly.
        services.AddSingleton<VariablesPanelViewModel>();
        services.AddSingleton<ConstantsPanelViewModel>();
        services.AddSingleton<InputUnitsPanelViewModel>();
        services.AddSingleton<OutputUnitsPanelViewModel>();
        services.AddSingleton<FunctionsPanelViewModel>();
        services.AddSingleton<BuiltInFunctionsPanelViewModel>();
        services.AddSingleton<CommandPaletteViewModel>();

        services.AddSingleton<ShellViewModel>();

        var serviceProvider = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new ShellWindow
            {
                DataContext = serviceProvider.GetRequiredService<ShellViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
