using FluentAssertions;
using MathVerse.Math.Semantics;
using MathVerse.Math.Semantics.Binding;
using MathVerse.Math.Semantics.Builtins;
using MathVerse.Math.Semantics.Diagnostics;
using MathVerse.Math.Semantics.Resolution;
using MathVerse.Math.Semantics.Symbols;

namespace MathVerse.Semantics.Tests;

public class SemanticModelTests
{
    private static SemanticModel Analyze(string input)
    {
        return new SemanticAnalyzer().Analyze(input);
    }

    [Fact]
    public void Model_Literal_Succeeds()
    {
        var model = Analyze("42");
        model.Success.Should().BeTrue();
        model.BoundTree.Should().BeOfType<BoundLiteralExpression>();
    }

    [Fact]
    public void Model_NodeCount()
    {
        var model = Analyze("2 + 3");
        model.NodeCount.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void Model_EvaluateConstant_Simple()
    {
        var model = Analyze("2 + 3");
        model.EvaluateConstant().Should().Be(5.0);
    }

    [Fact]
    public void Model_EvaluateConstant_Complex()
    {
        var model = Analyze("(2 + 3) * 4");
        model.EvaluateConstant().Should().Be(20.0);
    }

    [Fact]
    public void Model_EvaluateConstant_Function()
    {
        var model = Analyze("sqrt(9)");
        model.EvaluateConstant().Should().Be(3.0);
    }

    [Fact]
    public void Model_SymbolCount()
    {
        var model = Analyze("42");
        model.SymbolCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Model_ReferenceCount()
    {
        var model = Analyze("sin(1)");
        model.ReferenceCount.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Model_FoldConstants()
    {
        var model = Analyze("2 + 3");
        var folded = model.FoldConstants();
        folded.Should().BeOfType<BoundLiteralExpression>();
    }

    [Fact]
    public void Model_GetDiagnostics()
    {
        var model = Analyze("42");
        model.GetDiagnostics(SemanticSeverity.Error).Should().NotBeNull();
    }

    [Fact]
    public void Model_AnalyzeExpression()
    {
        var analyzer = new SemanticAnalyzer();
        var expr = MathVerse.Math.Expressions.Expr.Literal(5.0);
        var model = analyzer.AnalyzeExpression(expr);
        model.Success.Should().BeTrue();
    }

    [Fact]
    public void Model_CustomSymbols()
    {
        var analyzer = new SemanticAnalyzer();
        var model = analyzer.Analyze("x + 1", table =>
        {
            table.Declare(new VariableSymbol("x"));
        });
        model.Success.Should().BeTrue();
    }

    [Fact]
    public void Model_IsSymbolUsed()
    {
        var model = Analyze("sin(1)");
        model.IsSymbolUsed("sin").Should().BeTrue();
    }

    [Fact]
    public void Model_BuiltinRegistry_Constants()
    {
        var table = new SymbolTable();
        table.Lookup("π").Should().NotBeNull();
        ((ConstantSymbol)table.Lookup("π")!).Value.Should().BeApproximately(3.141592653589793, 1e-10);
    }

    [Fact]
    public void Model_BuiltinRegistry_PhysicsConstants()
    {
        var analyzer = new SemanticAnalyzer();
        var model = analyzer.AnalyzeExpression(
            MathVerse.Math.Expressions.Expr.Constant("g", 9.80665));
        model.EvaluateConstant().Should().BeApproximately(9.80665, 1e-4);
    }

    [Fact]
    public void Model_MultipleExpressions()
    {
        var expressions = new[] { "1 + 1", "2 * 3", "sqrt(4)", "sin(0)", "2^10" };
        foreach (var expr in expressions)
        {
            var model = Analyze(expr);
            model.Success.Should().BeTrue();
            model.EvaluateConstant().Should().NotBeNull();
        }
    }

    [Fact]
    public void Model_UndefinedSymbol_Diagnostic()
    {
        var model = Analyze("unknownVar");
        model.Success.Should().BeFalse();
        model.Diagnostics.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Model_UndefinedFunction_Diagnostic()
    {
        var model = Analyze("notAFunction(1)");
        model.Success.Should().BeFalse();
    }

    [Fact]
    public void Model_ReferenceGraph_Populated()
    {
        var model = Analyze("sin(1)");
        model.ReferenceGraph.AllReferences.Should().NotBeEmpty();
    }

    [Fact]
    public void Model_AnalyzeWithConfiguredSymbols()
    {
        var analyzer = new SemanticAnalyzer();
        var model = analyzer.Analyze("x + y", table =>
        {
            table.Declare(new VariableSymbol("x"));
            table.Declare(new VariableSymbol("y"));
        });
        model.Success.Should().BeTrue();
    }
}
