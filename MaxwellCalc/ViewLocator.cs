using Avalonia.Controls;
using Avalonia.Controls.Templates;
using MaxwellCalc.ViewModels;
using System;

namespace MaxwellCalc;

/// <summary>
/// The view locator.
/// </summary>
public class ViewLocator : IDataTemplate
{
    /// <inheritdoc />
    public bool SupportsRecycling => false;

    /// <inheritdoc />
    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }

    /// <inheritdoc />
    public Control? Build(object? param)
    {
        // Get the name of the view
        var name = param?.GetType()?.FullName?.Replace("ViewModel", "View");
        if (name is null)
            return null;

        // Use the derived name to get the view
        var type = Type.GetType(name);
        if (type is null)
            return new TextBlock { Text = $"View not found for {param?.GetType().Name ?? "Invalid type"}" };
        else
            return (Control?)Activator.CreateInstance(type);
    }
}
