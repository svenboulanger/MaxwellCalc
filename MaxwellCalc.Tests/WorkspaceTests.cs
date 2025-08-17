using MaxwellCalc.Core.Domains;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Core.Workspaces.Variables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MaxwellCalc.Tests
{
    public class WorkspaceTests
    {
        private readonly JsonSerializerOptions _options;

        public WorkspaceTests()
        {
            _options = new JsonSerializerOptions { WriteIndented = false };
            _options.Converters.Add(new WorkspaceJsonConverter<double>(() => new Workspace<double>(new DoubleDomain())));
        }

        [Fact]
        public void When_WorkspaceToJSON_Expect_Reference()
        {
            var workspace = new Workspace<double>(new DoubleDomain());

            // Add a few input units
            workspace.InputUnits.Add("m", new(1.0, Unit.UnitMeter));
            workspace.InputUnits.Add("um", new(1e-6, Unit.UnitMeter));

            // Add a few output units
            workspace.OutputUnits.Add(new(new Unit(("m", 1)), Unit.UnitMeter), 1.0);
            workspace.OutputUnits.Add(new(new Unit(("um", 1)), Unit.UnitMeter), 1e-6);

            // Add a few constants
            ((IVariableScope<double>)workspace.Constants).Local["pi"] = new(new(Math.PI, Unit.UnitNone), "Pi");
            ((IVariableScope<double>)workspace.Constants).Local["e"] = new(new(Math.E, Unit.UnitNone), "Euler's constant.");

            // Add a few variables
            ((IVariableScope<double>)workspace.Variables).Local["a"] = new(new(2.0, Unit.UnitMeter), null);
            ((IVariableScope<double>)workspace.Variables).Local["b"] = new(new(10.5, Unit.UnitNone), null);

            // Serialize and deserialize
            string json = JsonSerializer.Serialize<IWorkspace<double>>(workspace, _options);
            var newWorkspace = JsonSerializer.Deserialize<IWorkspace<double>>(json, _options) ?? throw new ArgumentNullException();

            // Now let's compare the two workspaces
            Assert.Equal(workspace.InputUnits.Count, newWorkspace.InputUnits.Count);
            foreach (var pair in workspace.InputUnits)
            {
                if (!newWorkspace.InputUnits.TryGetValue(pair.Key, out var reference))
                    throw new ArgumentException();
                if (!reference.Equals(pair.Value))
                    throw new ArgumentException();
            }
            Assert.Equal(workspace.OutputUnits.Count, newWorkspace.OutputUnits.Count);
            foreach (var pair in workspace.OutputUnits)
            {
                if (!newWorkspace.OutputUnits.TryGetValue(pair.Key, out var reference))
                    throw new ArgumentException();
                if (!reference.Equals(pair.Value))
                    throw new ArgumentException();
            }
            Assert.Equal(workspace.Constants.Local.Count, newWorkspace.Constants.Local.Count);
            foreach (var pair in ((IVariableScope<double>)workspace.Constants).Local)
            {
                if (!((IVariableScope<double>)newWorkspace.Constants).Local.TryGetValue(pair.Key, out var reference))
                    throw new ArgumentException();
                if (!reference.Equals(pair.Value))
                    throw new ArgumentException();
            }
            Assert.Equal(workspace.Variables.Local.Count, newWorkspace.Variables.Local.Count);
            foreach (var pair in ((IVariableScope<double>)workspace.Variables).Local)
            {
                if (!((IVariableScope<double>)newWorkspace.Variables).Local.TryGetValue(pair.Key, out var reference))
                    throw new ArgumentException();
                if (!reference.Equals(pair.Value))
                    throw new ArgumentException();
            }
        }
    }
}
