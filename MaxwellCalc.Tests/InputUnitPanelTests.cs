using MaxwellCalc.Core.Parsers;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Notebook.ViewModels;
using System.Text.Json;

namespace MaxwellCalc.Tests;

/// <summary>
/// Covers the command palette's "New input unit" footer (<c>InputUnitsPanelViewModel.Add</c>): defining a
/// brand-new base unit (blank definition), defining a derived unit in terms of known units, and the
/// diagnostic surfaced when the definition names an unknown unit.
/// </summary>
public class InputUnitPanelTests
{
    // Mirrors InputUnitsPanelViewModel.Add: blank definition => base unit, else parse + assign.
    private static (bool ok, string diag) PanelAdd(IWorkspace workspace, string symbol, string? definition)
    {
        var diagnostics = new List<string>();
        void Collect(object? s, DiagnosticMessagePostedEventArgs e) => diagnostics.Add(e.Message);
        workspace.DiagnosticMessagePosted += Collect;
        bool ok;
        try
        {
            if (string.IsNullOrWhiteSpace(definition))
            {
                ok = workspace.TryAssignBaseUnit(symbol);
            }
            else
            {
                var oldState = workspace.Restrict(false, false, true, false, false);
                try
                {
                    var node = Parser.Parse(new Lexer(definition), workspace);
                    ok = node is not null && workspace.TryAssignInputUnit(symbol, node);
                }
                finally { workspace.Restore(oldState); }
            }
        }
        finally { workspace.DiagnosticMessagePosted -= Collect; }
        return (ok, string.Join(" | ", diagnostics));
    }

    [Theory]
    [InlineData("px")]
    [InlineData("DN")]
    [InlineData("e")]
    [InlineData("LSB")]
    public void BaseUnit_RegistersUsesRendersAndPersists(string symbol)
    {
        var workspace = WorkspaceState.CreateDefaultWorkspace();

        // Register the brand-new base unit via the panel's blank-definition path.
        var (ok, diag) = PanelAdd(workspace, symbol, null);
        Assert.True(ok, diag);
        Assert.True(workspace.InputUnits.ContainsKey(symbol));

        // It must be its OWN dimension, not dimensionless.
        Assert.True(workspace.InputUnits.TryGetValue(symbol, out var def));
        Assert.Equal(symbol, def.Unit.ToString());

        // Using it in an expression resolves and renders back with the symbol.
        var node = Parser.Parse(new Lexer($"5 {symbol}"), workspace);
        Assert.NotNull(node);
        Assert.True(workspace.TryResolveAndFormat(node!, out var formatted), "resolve+format failed");
        Assert.Equal(symbol, formatted.Unit.ToString());

        // It survives a workspace.json round-trip.
        var options = new JsonSerializerOptions { WriteIndented = true };
        options.Converters.Add(new WorkspaceJsonConverter());
        options.Converters.Add(new WorkspaceJsonConverterFactory());
        string json = JsonSerializer.Serialize(workspace, typeof(IWorkspace), options);
        var restored = JsonSerializer.Deserialize<IWorkspace>(json, options)!;
        Assert.True(restored.InputUnits.ContainsKey(symbol));
    }

    [Fact]
    public void BaseUnit_DuplicateSymbol_Fails()
    {
        var workspace = WorkspaceState.CreateDefaultWorkspace();
        var (ok, diag) = PanelAdd(workspace, "m", null); // m already exists
        Assert.False(ok);
        Assert.Contains("already", diag);
    }

    [Theory]
    [InlineData("in", "0.0254 m")]
    [InlineData("inch", "2.54 cm")]
    public void DerivedUnit_StillWorks(string symbol, string definition)
    {
        var workspace = WorkspaceState.CreateDefaultWorkspace();
        var (ok, diag) = PanelAdd(workspace, symbol, definition);
        Assert.True(ok, diag);
    }

    [Fact]
    public void UnknownSymbol_ErrorNamesTheSymbol()
    {
        var workspace = WorkspaceState.CreateDefaultWorkspace();
        var (ok, diag) = PanelAdd(workspace, "furlong", "220 yard");
        Assert.False(ok);
        Assert.Contains("yard", diag);                 // names the real offender
        Assert.DoesNotContain("UnitNode", diag);       // not the old garbage message
    }
}
