using MaxwellCalc.Notebook.ViewModels;

namespace MaxwellCalc.Tests;

/// <summary>
/// Tests the sheet's whole-row selection (Shift+Up/Down, drag across rows) and deleting the selected rows.
/// </summary>
public class SheetRowSelectionTests
{
    private static SheetViewModel CreateSheet(params string[] lines)
    {
        var sheet = new SheetViewModel(new WorkspaceState());
        sheet.SetLines(lines);
        return sheet;
    }

    private static bool[] Selected(SheetViewModel sheet) => sheet.Lines.Select(line => line.IsSelected).ToArray();

    private static string[] Texts(SheetViewModel sheet) => sheet.Lines.Select(line => line.Text).ToArray();

    [Fact]
    public void ExtendRowSelection_FirstPress_SelectsCurrentAndNeighbour()
    {
        var sheet = CreateSheet("1", "2", "3", "4");

        sheet.ExtendRowSelection(1, 1);

        Assert.Equal([false, true, true, false], Selected(sheet));
        Assert.Equal(1, sheet.SelectionAnchor);
        Assert.Equal(2, sheet.SelectionActive);
    }

    [Fact]
    public void ExtendRowSelection_AtSheetEdge_SelectsOnlyCurrentLine()
    {
        var sheet = CreateSheet("1", "2", "3");

        sheet.ExtendRowSelection(2, 1);

        Assert.Equal([false, false, true], Selected(sheet));
    }

    [Fact]
    public void ExtendRowSelection_MovesActiveEndAcrossAnchor()
    {
        var sheet = CreateSheet("1", "2", "3", "4");

        sheet.ExtendRowSelection(2, 1);   // 2..3
        sheet.ExtendRowSelection(-1, -1); // 2..2
        sheet.ExtendRowSelection(-1, -1); // 1..2

        Assert.Equal([false, true, true, false], Selected(sheet));
        Assert.Equal(2, sheet.SelectionAnchor);
        Assert.Equal(1, sheet.SelectionActive);
    }

    [Fact]
    public void DeleteSelectedLines_Backspace_RemovesRowsAndFocusesLineAbove()
    {
        var sheet = CreateSheet("1", "2", "3", "4");
        int? focused = null;
        sheet.FocusRequested += index => focused = index;

        sheet.SelectRows(1, 2);
        sheet.DeleteSelectedLines(backward: true);

        Assert.Equal(["1", "4"], Texts(sheet));
        Assert.False(sheet.HasRowSelection);
        Assert.Equal(0, focused);
        Assert.Equal(1, sheet.Lines[0].CaretIndex);
    }

    [Fact]
    public void DeleteSelectedLines_Delete_FocusesLineThatTookTheirPlace()
    {
        var sheet = CreateSheet("1", "2", "3", "4");
        int? focused = null;
        sheet.FocusRequested += index => focused = index;

        sheet.SelectRows(2, 1);
        sheet.DeleteSelectedLines(backward: false);

        Assert.Equal(["1", "4"], Texts(sheet));
        Assert.Equal(1, focused);
        Assert.Equal(0, sheet.Lines[1].CaretIndex);
    }

    [Fact]
    public void DeleteSelectedLines_AllRows_LeavesSingleEmptyLine()
    {
        var sheet = CreateSheet("1", "2", "3");

        sheet.SelectRows(0, 2);
        sheet.DeleteSelectedLines(backward: true);

        Assert.Equal([""], Texts(sheet));
        Assert.False(sheet.Lines[0].IsSelected);
    }

    [Fact]
    public void DeleteSelectedLines_ReevaluatesRemainingLines()
    {
        var sheet = CreateSheet("x = 2", "x = 3", "x");

        sheet.SelectRows(1, 1);
        sheet.DeleteSelectedLines(backward: true);

        Assert.Equal("2", sheet.Lines[1].Quantity.Scalar);
    }

    [Fact]
    public void LinesChanging_ClearsSelection()
    {
        var sheet = CreateSheet("1", "2", "3");

        sheet.SelectRows(0, 1);
        sheet.SplitLine(2, 1);

        Assert.False(sheet.HasRowSelection);
        Assert.All(sheet.Lines, line => Assert.False(line.IsSelected));
    }
}
