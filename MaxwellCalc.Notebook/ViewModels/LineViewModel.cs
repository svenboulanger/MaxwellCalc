using CommunityToolkit.Mvvm.ComponentModel;
using MaxwellCalc.Core.Units;
using MaxwellCalc.Notebook.Evaluation;

namespace MaxwellCalc.Notebook.ViewModels;

/// <summary>
/// A single line in the notebook sheet: the editable text plus the derived result state that is
/// recomputed by <see cref="SheetViewModel"/> whenever any line changes.
/// </summary>
public partial class LineViewModel : ViewModelBase
{
    /// <summary>
    /// Gets or sets the raw expression text of the line.
    /// </summary>
    [ObservableProperty]
    private string _text = string.Empty;

    /// <summary>
    /// Gets or sets the caret position within the line, used by the keyboard model (Step 7).
    /// </summary>
    [ObservableProperty]
    [property: System.Text.Json.Serialization.JsonIgnore]
    private int _caretIndex;

    /// <summary>
    /// Gets the kind of result the line produced.
    /// </summary>
    [ObservableProperty]
    [property: System.Text.Json.Serialization.JsonIgnore]
    [NotifyPropertyChangedFor(nameof(HasValue))]
    private LineKind _kind;

    /// <summary>
    /// Gets the formatted quantity for value / assignment lines.
    /// </summary>
    [ObservableProperty]
    [property: System.Text.Json.Serialization.JsonIgnore]
    private Quantity<string> _quantity;

    /// <summary>
    /// Gets whether the line is a single constant identifier (renders a <c>const</c> pill).
    /// </summary>
    [ObservableProperty]
    [property: System.Text.Json.Serialization.JsonIgnore]
    private bool _isConstBadge;

    /// <summary>
    /// Gets whether the output unit was auto-selected (for the Step 6 caption).
    /// </summary>
    [ObservableProperty]
    [property: System.Text.Json.Serialization.JsonIgnore]
    private bool _autoUnitSelected;

    /// <summary>
    /// Gets the joined diagnostic message for error lines, or <c>null</c>.
    /// </summary>
    [ObservableProperty]
    [property: System.Text.Json.Serialization.JsonIgnore]
    private string? _errorMessage;

    /// <summary>
    /// Gets whether this line has a value to render in the gutter.
    /// </summary>
    public bool HasValue => Kind is LineKind.Value or LineKind.Assign;

    /// <summary>
    /// Creates a new empty <see cref="LineViewModel"/>.
    /// </summary>
    public LineViewModel()
    {
    }

    /// <summary>
    /// Creates a new <see cref="LineViewModel"/> with the given text.
    /// </summary>
    /// <param name="text">The initial line text.</param>
    public LineViewModel(string text)
    {
        _text = text;
    }

    /// <summary>
    /// Copies an evaluation result onto this line.
    /// </summary>
    /// <param name="result">The evaluated result.</param>
    public void ApplyResult(LineResult result)
    {
        Kind = result.Kind;
        Quantity = result.Quantity;
        IsConstBadge = result.IsConstBadge;
        AutoUnitSelected = result.AutoUnitSelected;
        ErrorMessage = result.ErrorMessage;
    }
}
