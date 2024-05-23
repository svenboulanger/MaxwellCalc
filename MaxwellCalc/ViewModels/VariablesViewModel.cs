using System.Collections.ObjectModel;
using Avalonia.Controls;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A view model for the list of variables.
    /// </summary>
    public class VariablesViewModel
    {
        /// <summary>
        /// The collection of variables.
        /// </summary>
        public ObservableCollection<VariableViewModel> List { get; } = new();

        /// <summary>
        /// Creates a new <see cref="VariablesViewModel"/>.
        /// </summary>
        public VariablesViewModel()
        {
            if (Design.IsDesignMode)
            {
                for (int i = 0; i < 100; i++)
                    List.Add(new() { Name = $"Test {i}", Value = new Quantity<string>("0", new((Unit.Kelvin, 2))) });
            }
        }

        /// <summary>
        /// Updates the variable list with that of the given workspace.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public void Update(IWorkspace workspace)
        {
            List.Clear();
            foreach (var pair in workspace.Variables)
            {
                List.Add(new() { Name = pair.Name, Value = pair.Value });
            }
        }
    }
}
