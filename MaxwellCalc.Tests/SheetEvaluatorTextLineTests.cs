using MaxwellCalc.Core.Workspaces;
using MaxwellCalc.Notebook.Evaluation;
using MaxwellCalc.Notebook.ViewModels;

namespace MaxwellCalc.Tests;

/// <summary>
/// Tests for prose ("# …") lines: the marker/brace grammar, inline expression evaluation, and the
/// guarantee that inline assignments thread through the pass yet leave the persistent workspace intact.
/// </summary>
public class SheetEvaluatorTextLineTests
{
    private const string Format = "g12";

    private static IWorkspace NewWorkspace() => WorkspaceState.CreateDefaultWorkspace();

    [Fact]
    public void When_PlainProse_Expect_SingleLiteralSegment()
    {
        var results = SheetEvaluator.Evaluate(NewWorkspace(), ["# just a note"], Format);

        Assert.Equal(LineKind.Text, results[0].Kind);
        var segments = results[0].Segments!;
        Assert.Single(segments);
        Assert.Equal("just a note", segments[0].Literal);
    }

    [Fact]
    public void When_InlineExpression_Expect_EvaluatedBetweenLiterals()
    {
        var results = SheetEvaluator.Evaluate(NewWorkspace(), ["# speed is {1 m/s in km/hour} today"], Format);

        Assert.Equal(LineKind.Text, results[0].Kind);
        var segments = results[0].Segments!;
        Assert.Equal(3, segments.Count);
        Assert.Equal("speed is ", segments[0].Literal);
        Assert.Equal(LineKind.Value, segments[1].Expression!.Value.Kind);
        Assert.Equal("3.6", segments[1].Expression!.Value.Quantity.Scalar);
        Assert.Equal(" today", segments[2].Literal);
    }

    [Fact]
    public void When_InlineAssignment_Expect_ThreadedAndTransient()
    {
        var workspace = NewWorkspace();
        var variableScope = (Core.Workspaces.Variables.IVariableScope<double>)workspace.Variables;

        var results = SheetEvaluator.Evaluate(workspace, ["# let {x = 5}", "x * 2"], Format);

        // The inline assignment renders its value and threads x into the later line.
        Assert.Equal(LineKind.Text, results[0].Kind);
        Assert.Equal(LineKind.Assign, results[0].Segments![1].Expression!.Value.Kind);
        Assert.Equal("5", results[0].Segments![1].Expression!.Value.Quantity.Scalar);

        Assert.Equal(LineKind.Value, results[1].Kind);
        Assert.Equal("10", results[1].Quantity.Scalar);

        // …but nothing leaks into the persistent workspace.
        Assert.False(variableScope.Local.ContainsKey("x"));
    }

    [Fact]
    public void When_InlineFunctionDefinition_Expect_ThreadedAndSignature()
    {
        var workspace = NewWorkspace();

        var results = SheetEvaluator.Evaluate(workspace, ["# define {f(x) = x^2}", "f(3)"], Format);

        var funcSegment = results[0].Segments![1].Expression!.Value;
        Assert.Equal(LineKind.FuncDef, funcSegment.Kind);
        Assert.Equal("f(x)", funcSegment.DefinedName);

        Assert.Equal(LineKind.Value, results[1].Kind);
        Assert.Equal("9", results[1].Quantity.Scalar);

        Assert.False(workspace.UserFunctions.ContainsKey(new UserFunctionKey("f", 1)));
    }

    [Fact]
    public void When_EscapedBraces_Expect_LiteralBraces()
    {
        var results = SheetEvaluator.Evaluate(NewWorkspace(), ["# a {{b}} c"], Format);

        var segments = results[0].Segments!;
        Assert.Single(segments);
        Assert.Equal("a {b} c", segments[0].Literal);
    }

    [Fact]
    public void When_UnclosedBrace_Expect_LiteralRemainder()
    {
        var results = SheetEvaluator.Evaluate(NewWorkspace(), ["# a {b c"], Format);

        var segments = results[0].Segments!;
        Assert.Single(segments);
        Assert.Equal("a {b c", segments[0].Literal);
    }

    [Fact]
    public void When_InlineExpressionInvalid_Expect_ErrorSegment()
    {
        var results = SheetEvaluator.Evaluate(NewWorkspace(), ["# value {foo + 1}"], Format);

        var errorSegment = results[0].Segments![1];
        Assert.Equal(LineKind.Error, errorSegment.Expression!.Value.Kind);
        Assert.Equal("{foo + 1}", errorSegment.RawSource);
        Assert.NotNull(errorSegment.Expression!.Value.ErrorMessage);
    }

    [Fact]
    public void When_EmptyBraces_Expect_LiteralBraces()
    {
        var results = SheetEvaluator.Evaluate(NewWorkspace(), ["# nothing {} here"], Format);

        // An empty {} carries nothing to evaluate; it stays literal text.
        var segments = results[0].Segments!;
        Assert.DoesNotContain(segments, s => s.Expression is not null);
    }

    [Fact]
    public void When_MarkerHasNoLeadingSpace_Expect_ContentFromMarker()
    {
        var results = SheetEvaluator.Evaluate(NewWorkspace(), ["#tight"], Format);

        Assert.Equal(LineKind.Text, results[0].Kind);
        Assert.Equal("tight", results[0].Segments![0].Literal);
    }
}
