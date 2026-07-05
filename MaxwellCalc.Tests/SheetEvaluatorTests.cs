using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Notebook.Evaluation;
using MaxwellCalc.Notebook.ViewModels;

namespace MaxwellCalc.Tests;

/// <summary>
/// Regression tests for the notebook sheet evaluation loop (Step 4). The highest-risk item is that
/// evaluating the sheet must never mutate the persistent workspace.
/// </summary>
public class SheetEvaluatorTests
{
    private const string Format = "g12";

    private static IWorkspace NewWorkspace() => WorkspaceState.CreateDefaultWorkspace();

    [Fact]
    public void When_UnitConversion_Expect_Converted()
    {
        var results = SheetEvaluator.Evaluate(NewWorkspace(), ["1 m/s in km/hour"], Format);

        Assert.Equal(LineKind.Value, results[0].Kind);
        Assert.Equal("3.6", results[0].Quantity.Scalar);
    }

    [Fact]
    public void When_AssignmentThenReference_Expect_ThreadedResult()
    {
        var workspace = NewWorkspace();

        var results = SheetEvaluator.Evaluate(workspace, ["mass = 70 kg", "mass * 2"], Format);

        Assert.Equal(LineKind.Assign, results[0].Kind);
        Assert.Equal(LineKind.Value, results[1].Kind);

        // "mass * 2" must render identically to evaluating "140 kg" directly (both pass through the
        // same output-unit resolution).
        var reference = SheetEvaluator.Evaluate(NewWorkspace(), ["140 kg"], Format);
        Assert.Equal(reference[0].Quantity, results[1].Quantity);
    }

    [Fact]
    public void When_FunctionDefinitionThenCall_Expect_Evaluated()
    {
        var workspace = NewWorkspace();

        var results = SheetEvaluator.Evaluate(workspace, ["f(x) = x^2", "f(3)"], Format);

        Assert.Equal(LineKind.FuncDef, results[0].Kind);
        Assert.Equal(LineKind.Value, results[1].Kind);
        Assert.Equal("9", results[1].Quantity.Scalar);
    }

    [Fact]
    public void When_SheetEvaluated_Expect_WorkspaceUnchanged()
    {
        var workspace = NewWorkspace();
        var variableScope = (Core.Workspaces.Variables.IVariableScope<double>)workspace.Variables;

        var variablesBefore = variableScope.Local.Keys.OrderBy(k => k).ToList();
        var functionsBefore = workspace.UserFunctions.Keys.OrderBy(k => k.Name).ToList();

        SheetEvaluator.Evaluate(
            workspace,
            ["mass = 70 kg", "mass * 2", "f(x) = x^2", "f(3)"],
            Format);

        var variablesAfter = variableScope.Local.Keys.OrderBy(k => k).ToList();
        var functionsAfter = workspace.UserFunctions.Keys.OrderBy(k => k.Name).ToList();

        Assert.Equal(variablesBefore, variablesAfter);
        Assert.Equal(functionsBefore, functionsAfter);
        Assert.False(variableScope.Local.ContainsKey("mass"));
        Assert.False(workspace.UserFunctions.ContainsKey(new UserFunctionKey("f", 1)));
    }

    [Fact]
    public void When_ExpressionInvalid_Expect_DiagnosticReported()
    {
        var results = SheetEvaluator.Evaluate(NewWorkspace(), ["foo + 1"], Format);

        Assert.Equal(LineKind.Error, results[0].Kind);
        Assert.NotNull(results[0].ErrorMessage);
        Assert.Contains("foo", results[0].ErrorMessage);
    }

    [Fact]
    public void When_InlineAssignment_Expect_ThreadedAndTransient()
    {
        var workspace = NewWorkspace();
        var variableScope = (Core.Workspaces.Variables.IVariableScope<double>)workspace.Variables;

        // An assignment embedded inside a larger expression (Core allows this): the root node is not
        // an Assign, so the line is a Value, but the embedded binding must still thread to later lines
        // and must not leak into the persistent workspace.
        var results = SheetEvaluator.Evaluate(workspace, ["(a = 3) + 1", "a * 2"], Format);

        Assert.Equal(LineKind.Value, results[0].Kind);
        Assert.Equal("4", results[0].Quantity.Scalar);
        Assert.Equal(LineKind.Value, results[1].Kind);
        Assert.Equal("6", results[1].Quantity.Scalar);

        Assert.False(variableScope.Local.ContainsKey("a"));
    }

    [Fact]
    public void When_ConstantAlone_Expect_ConstBadge()
    {
        var results = SheetEvaluator.Evaluate(NewWorkspace(), ["pi"], Format);

        Assert.Equal(LineKind.Value, results[0].Kind);
        Assert.True(results[0].IsConstBadge);
    }

    [Fact]
    public void When_EmptyLine_Expect_Empty()
    {
        var results = SheetEvaluator.Evaluate(NewWorkspace(), ["", "   "], Format);

        Assert.Equal(LineKind.Empty, results[0].Kind);
        Assert.Equal(LineKind.Empty, results[1].Kind);
    }
}
