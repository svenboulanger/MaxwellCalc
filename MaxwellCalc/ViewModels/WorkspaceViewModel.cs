using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Core.Workspaces;
using System.Text.Json.Serialization;

namespace MaxwellCalc.ViewModels;

/// <summary>
/// A view model centered around an <see cref="IWorkspace"/>.
/// </summary>
public partial class WorkspaceViewModel : SelectableViewModelBase<IWorkspace>
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private int _formatIndex = 0;

    [ObservableProperty]
    private int _digits = 12;

    [ObservableProperty]
    private DomainTypes _domainType;

    /// <summary>
    /// Gets the format string that is specified by the workspace.
    /// </summary>
    [JsonIgnore]
    public string OutputFormat
    {
        get
        {
            return FormatIndex switch
            {
                // Scientific
                1 => $"e{Digits}",
                // Fixed
                2 => $"f{Digits}",
                _ => $"g{Digits}",
            };
        }
    }

    /// <summary>
    /// Creates a new <see cref="WorkspaceViewModel"/>.
    /// </summary>
    public WorkspaceViewModel()
    {
    }
}
