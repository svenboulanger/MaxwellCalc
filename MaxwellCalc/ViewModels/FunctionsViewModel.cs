using Avalonia.Controls;
using MaxwellCalc.Units;
using MaxwellCalc.Workspaces;
using System.Collections.ObjectModel;

namespace MaxwellCalc.ViewModels
{
    /// <summary>
    /// A view model for the function list.
    /// </summary>
    public class FunctionsViewModel
    {
        /// <summary>
        /// The collection of functions.
        /// </summary>
        public ObservableCollection<FunctionViewModel> List { get; } = new();

        /// <summary>
        /// Creates a new <see cref="FunctionsViewModel"/>.
        /// </summary>
        public FunctionsViewModel()
        {
            if (Design.IsDesignMode)
            {
                for (int i = 0; i < 100; i++)
                    List.Add(new() { Name = $"Test{i}(a, b)", Value = "0" });
            }
        }

        /// <summary>
        /// Updates the function list with that of the workspace.
        /// </summary>
        /// <param name="workspace">The workspace.</param>
        public void Update(IWorkspace workspace)
        {
            List.Clear();
            foreach (var pair in workspace.UserFunctions)
            {
                string name = $"{pair.Name}({string.Join(",", pair.Parameters)})";
                string value = pair.Body;
                List.Add(new() { Name = name, Value = value });
            }
        }
    }
}
