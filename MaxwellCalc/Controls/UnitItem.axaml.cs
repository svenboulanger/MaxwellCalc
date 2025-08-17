using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Media;
using MaxwellCalc.Core.Units;
using System;

namespace MaxwellCalc.Controls;

public class UnitItem : TemplatedControl
{
    private SelectableTextBlock? _descriptionBlock;
    private Button? _removeButton = null;

    public static readonly StyledProperty<Unit> LeftProperty =
        AvaloniaProperty.Register<UnitItem, Unit>(nameof(Left), new Unit(("Unit", 1)));
    public static readonly StyledProperty<Quantity<string>> QuantityProperty =
        AvaloniaProperty.Register<UnitItem, Quantity<string>>(nameof(Quantity), new Quantity<string>("Default", Unit.UnitNone));
    public static readonly StyledProperty<IBrush?> UnitForegroundProperty =
        AvaloniaProperty.Register<UnitItem, IBrush?>(nameof(UnitForeground), Brushes.Black);
    public static readonly StyledProperty<IBrush?> ScalarForegroundProperty =
        AvaloniaProperty.Register<UnitItem, IBrush?>(nameof(ScalarForeground), Brushes.Gray);
    public static readonly StyledProperty<IBrush?> BaseUnitForegroundProperty =
        AvaloniaProperty.Register<UnitItem, IBrush?>(nameof(BaseUnitForeground), Brushes.Red);

    public IBrush? UnitForeground
    {
        get => GetValue(UnitForegroundProperty);
        set => SetValue(UnitForegroundProperty, value);
    }

    public IBrush? ScalarForeground
    {
        get => GetValue(ScalarForegroundProperty);
        set => SetValue(ScalarForegroundProperty, value);
    }

    public IBrush? BaseUnitForeground
    {
        get => GetValue(BaseUnitForegroundProperty);
        set => SetValue(BaseUnitForegroundProperty, value);
    }

    public Unit Left
    {
        get => GetValue(LeftProperty);
        set => SetValue(LeftProperty, value);
    }

    public Quantity<string> Quantity
    {
        get => GetValue(QuantityProperty);
        set => SetValue(QuantityProperty, value);
    }

    public event EventHandler<RoutedEventArgs>? RemoveClicked;

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Find the relevant subcontrols
        _descriptionBlock = e.NameScope.Find<SelectableTextBlock>("DescriptionBlock");
        _removeButton = e.NameScope.Find<Button>("RemoveButton");

        // Format
        FormatUnits();

        if (_removeButton is not null)
            _removeButton.Click += Remove_Clicked;
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property.Name == "Unit")
            FormatUnits();
    }

    private void FormatUnits()
    {
        if (_descriptionBlock is null)
            return;
        _descriptionBlock.Inlines ??= [];

        // Left-hand side
        UnitFormatter.Default.AppendInlinesFor(_descriptionBlock.Inlines, Left, UnitForeground, FontSize);

        // Right-hand side
        _descriptionBlock.Inlines.Add(new Run
        {
            Text = " = ",
            Foreground = UnitForeground,
            FontSize = FontSize * 0.75
        });
        _descriptionBlock.Inlines.Add(new Run
        {
            Text = Quantity.Scalar,
            Foreground = ScalarForeground,
            FontSize = FontSize * 0.75
        });
        UnitFormatter.Default.AppendInlinesFor(_descriptionBlock.Inlines, Quantity.Unit, BaseUnitForeground, FontSize * 0.75);
    }

    protected void Remove_Clicked(object? sender, RoutedEventArgs e) => RemoveClicked?.Invoke(this, e);
}