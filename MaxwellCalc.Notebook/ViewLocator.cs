using Avalonia.Controls;
using Avalonia.Controls.Templates;
using MaxwellCalc.Notebook.ViewModels;
using System;

namespace MaxwellCalc.Notebook;

/// <summary>
/// Maps a <c>*ViewModel</c> instance to its matching <c>*View</c> control by naming convention.
/// </summary>
public class ViewLocator : IDataTemplate
{
    /// <inheritdoc />
    public bool Match(object? data) => data is ViewModelBase;

    /// <inheritdoc />
    public Control? Build(object? param)
    {
        // Derive the view name from the view-model name (e.g. ...ViewModels.FooViewModel -> ...Views.FooView).
        var name = param?.GetType()?.FullName?.Replace("ViewModel", "View");
        if (name is null)
            return null;

        var type = Type.GetType(name);
        if (type is null)
            return new TextBlock { Text = $"View not found for {param?.GetType().Name ?? "Invalid type"}" };
        return (Control?)Activator.CreateInstance(type);
    }
}
