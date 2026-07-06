using MaxwellCalc.Core.Units;
using MaxwellCalc.Core.Workspaces;
using System.Collections.Generic;

namespace MaxwellCalc.Notebook.ViewModels.Overlay;

// The command-palette row view models (Step 8). Each is an immutable projection of one workspace
// dictionary entry (or one sheet-defined symbol). The panels rebuild their item lists wholesale on
// change, so the items themselves need no change notification.

/// <summary>
/// A variable row: a user-defined workspace variable, or a read-only <c>from sheet</c> variable.
/// </summary>
public sealed class VariableItem
{
    /// <summary>Gets the variable name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the formatted value.</summary>
    public Quantity<string> Value { get; init; }

    /// <summary>Gets whether the row shows a × remove button (false for <c>from sheet</c> rows).</summary>
    public bool CanRemove { get; init; }

    /// <summary>Gets whether this variable was defined on the sheet (shows the <c>from sheet</c> tag).</summary>
    public bool FromSheet { get; init; }
}

/// <summary>
/// A constant row: name, optional description and value. Constants can't be reassigned or removed.
/// </summary>
public sealed class ConstantItem
{
    /// <summary>Gets the constant name.</summary>
    public required string Name { get; init; }

    /// <summary>Gets the optional description.</summary>
    public string? Description { get; init; }

    /// <summary>Gets the formatted value.</summary>
    public Quantity<string> Value { get; init; }

    /// <summary>Gets whether a description is present (drives its visibility).</summary>
    public bool HasDescription => !string.IsNullOrWhiteSpace(Description);
}

/// <summary>
/// An input-unit row: the symbol and the base-unit definition it resolves to.
/// </summary>
public sealed class InputUnitItem
{
    /// <summary>Gets the unit symbol (rendered in the unit hue).</summary>
    public required string Symbol { get; init; }

    /// <summary>Gets the base-unit definition (<c>= …</c>).</summary>
    public Quantity<string> Definition { get; init; }

    /// <summary>Gets whether the row shows a × remove button.</summary>
    public bool CanRemove { get; init; } = true;
}

/// <summary>
/// An output-unit row: the output-unit label and the base-unit definition it maps from.
/// </summary>
public sealed class OutputUnitItem
{
    /// <summary>Gets the dictionary key, used to remove the row (Step 9).</summary>
    public required OutputUnitKey Key { get; init; }

    /// <summary>Gets the output-unit label (rendered in the unit hue).</summary>
    public required string Label { get; init; }

    /// <summary>Gets the base-unit definition (<c>= …</c>).</summary>
    public Quantity<string> Definition { get; init; }

    /// <summary>Gets whether the row shows a × remove button.</summary>
    public bool CanRemove { get; init; } = true;
}

/// <summary>
/// A group of output-unit rows sharing a physical quantity (Length, Mass, …), with a section label.
/// </summary>
public sealed class OutputUnitGroup
{
    /// <summary>Gets the physical-quantity label for the group.</summary>
    public required string Label { get; init; }

    /// <summary>Gets the rows in the group.</summary>
    public required IReadOnlyList<OutputUnitItem> Items { get; init; }
}

/// <summary>
/// A function row: a user-defined workspace function, or a read-only <c>from sheet</c> function.
/// </summary>
public sealed class FunctionItem
{
    /// <summary>Gets the dictionary key, used to remove the row (Step 9). <c>null</c> for <c>from sheet</c> rows.</summary>
    public UserFunctionKey? Key { get; init; }

    /// <summary>Gets the signature (<c>name(params)</c>).</summary>
    public required string Signature { get; init; }

    /// <summary>Gets the body text (<c>= …</c>), or <c>null</c> when unknown (e.g. sheet functions).</summary>
    public string? Body { get; init; }

    /// <summary>Gets whether the row shows a × remove button (false for <c>from sheet</c> rows).</summary>
    public bool CanRemove { get; init; }

    /// <summary>Gets whether this function was defined on the sheet (shows the <c>from sheet</c> tag).</summary>
    public bool FromSheet { get; init; }

    /// <summary>Gets whether the body is present (drives its visibility).</summary>
    public bool HasBody => !string.IsNullOrWhiteSpace(Body);
}

/// <summary>
/// A built-in function row: signature and description. Built-ins are always available and not removable.
/// </summary>
public sealed class BuiltInFunctionItem
{
    /// <summary>Gets the signature (<c>name(args)</c>).</summary>
    public required string Signature { get; init; }

    /// <summary>Gets the description.</summary>
    public string Description { get; init; } = string.Empty;
}
