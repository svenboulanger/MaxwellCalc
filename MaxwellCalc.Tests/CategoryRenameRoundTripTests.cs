using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Notebook.ViewModels;
using System.Text.Json;

namespace MaxwellCalc.Tests;

/// <summary>
/// Regression test for the output-unit category rename feature. Renaming a category writes
/// <c>workspace.UnitCategories[baseUnit] = newName</c> and (like the add/remove-unit flows) relies on the
/// existing <c>unit_categories</c> serialization to persist to <c>workspace.json</c>. The highest-risk item
/// is that a rename must survive the same round-trip the app performs on close/reopen.
/// </summary>
public class CategoryRenameRoundTripTests
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
    public void When_CategoryRenamedAndReloaded_Expect_RenamePreserved()
    {
        var workspace = WorkspaceState.CreateDefaultWorkspace();
        // Length is seeded to "length" (upper-cased to LENGTH for display); rename its base unit key.
        Assert.Equal("length", workspace.UnitCategories[Unit.UnitMeter]);
        workspace.UnitCategories[Unit.UnitMeter] = "Distances";

        var options = WorkspaceOptions();
        string json = JsonSerializer.Serialize(workspace, typeof(IWorkspace), options);
        var restored = JsonSerializer.Deserialize<IWorkspace>(json, options)!;

        Assert.Equal("Distances", restored.UnitCategories[Unit.UnitMeter]);
    }

    [Fact]
    public void When_MixedCaseName_Expect_CasingPreserved()
    {
        var workspace = WorkspaceState.CreateDefaultWorkspace();
        // A mixed-case name must be stored verbatim — display upper-cases it, but the underlying value
        // stays case-sensitive.
        workspace.UnitCategories[Unit.UnitMeter] = "Length of things";

        var options = WorkspaceOptions();
        string json = JsonSerializer.Serialize(workspace, typeof(IWorkspace), options);
        var restored = JsonSerializer.Deserialize<IWorkspace>(json, options)!;

        Assert.Equal("Length of things", restored.UnitCategories[Unit.UnitMeter]);
    }
}
