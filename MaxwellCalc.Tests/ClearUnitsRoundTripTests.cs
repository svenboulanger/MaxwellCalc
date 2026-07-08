using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Notebook.ViewModels;
using System.Text.Json;

namespace MaxwellCalc.Tests;

/// <summary>
/// Regression test for the "Clear units" action in the workspace-settings dialog. The highest-risk item is
/// that clearing must <em>persist</em>: a workspace with every input/output unit removed must round-trip
/// through the same serialization the app uses (<c>workspace.json</c>) and come back empty — not re-seeded
/// with its built-in units.
/// </summary>
public class ClearUnitsRoundTripTests
{
    // The workspace-serialization options the app builds in App.axaml.cs.
    private static JsonSerializerOptions WorkspaceOptions()
    {
        var options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(new WorkspaceJsonConverter());
        options.Converters.Add(new WorkspaceJsonConverterFactory());
        return options;
    }

    [Fact]
    public void When_AllUnitsClearedAndReloaded_Expect_StillEmpty()
    {
        var workspace = WorkspaceState.CreateDefaultWorkspace();
        Assert.NotEmpty(workspace.InputUnits);
        Assert.NotEmpty(workspace.OutputUnits);

        // The exact enumerate-and-remove the ClearUnits command performs.
        foreach (var key in workspace.InputUnits.Keys.ToList())
            workspace.TryRemoveInputUnit(key);
        foreach (var key in workspace.OutputUnits.Keys.ToList())
            workspace.TryRemoveOutputUnit(key);

        Assert.Empty(workspace.InputUnits);
        Assert.Empty(workspace.OutputUnits);

        var options = WorkspaceOptions();
        string json = JsonSerializer.Serialize(workspace, typeof(IWorkspace), options);
        var restored = JsonSerializer.Deserialize<IWorkspace>(json, options)!;

        Assert.Empty(restored.InputUnits);
        Assert.Empty(restored.OutputUnits);
    }
}
