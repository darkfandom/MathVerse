using FluentAssertions;
using MathVerse.Math.Semantics;
using MathVerse.Math.Semantics.Binding;
using MathVerse.Math.Semantics.Diagnostics;
using MathVerse.Math.Semantics.Resolution;
using MathVerse.Math.Semantics.Symbols;

namespace MathVerse.Semantics.Tests;

public class FunctionResolverTests
{
    private static FunctionResolver CreateResolver(out SemanticDiagnosticBag diagnostics)
    {
        diagnostics = new SemanticDiagnosticBag();
        var table = new SymbolTable();
        var context = new BindingContext(table, diagnostics);
        return new FunctionResolver(context);
    }

    [Fact]
    public void Resolve_SingleArgFunction()
    {
        var resolver = CreateResolver(out _);
        var result = resolver.Resolve("sin", 1);
        result.Should().NotBeNull();
        result!.Name.Should().Be("sin");
    }

    [Fact]
    public void Resolve_TwoArgFunction()
    {
        var resolver = CreateResolver(out _);
        var result = resolver.Resolve("pow", 2);
        result.Should().NotBeNull();
        result!.ParameterCount.Should().Be(2);
    }

    [Fact]
    public void Resolve_Undefined()
    {
        var resolver = CreateResolver(out var diag);
        var result = resolver.Resolve("noSuchFunc", 1);
        result.Should().BeNull();
        diag.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Resolve_TooFewArgs_Diagnostic()
    {
        var resolver = CreateResolver(out var diag);
        var result = resolver.Resolve("sin", 0);
        result.Should().NotBeNull();
        diag.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Resolve_TooManyArgs_Warning()
    {
        var resolver = CreateResolver(out var diag);
        var result = resolver.Resolve("sin", 5);
        result.Should().NotBeNull();
        diag.HasWarnings.Should().BeTrue();
    }

    [Fact]
    public void GetExpectedParameterCount()
    {
        var resolver = CreateResolver(out _);
        resolver.GetExpectedParameterCount("sin").Should().Be(1);
        resolver.GetExpectedParameterCount("pow").Should().Be(2);
    }

    [Fact]
    public void GetExpectedParameterCount_Unknown()
    {
        var resolver = CreateResolver(out _);
        resolver.GetExpectedParameterCount("unknown").Should().Be(-1);
    }
}
