using FluentAssertions;
using MathVerse.Math.Semantics;
using MathVerse.Math.Semantics.Binding;
using MathVerse.Math.Semantics.Diagnostics;
using MathVerse.Math.Semantics.Resolution;
using MathVerse.Math.Semantics.Symbols;

namespace MathVerse.Semantics.Tests;

public class IdentifierResolverTests
{
    private static IdentifierResolver CreateResolver(out SymbolTable table, out SemanticDiagnosticBag diagnostics)
    {
        diagnostics = new SemanticDiagnosticBag();
        table = new SymbolTable();
        var context = new BindingContext(table, diagnostics);
        return new IdentifierResolver(context);
    }

    [Fact]
    public void Resolve_KnownConstant()
    {
        var resolver = CreateResolver(out _, out _);
        var result = resolver.ResolveIdentifier("π");
        result.Should().BeOfType<BoundConstantExpression>();
    }

    [Fact]
    public void Resolve_UndefinedVariable()
    {
        var resolver = CreateResolver(out _, out var diag);
        var result = resolver.ResolveIdentifier("undefined");
        result.Should().BeOfType<BoundLiteralExpression>();
        diag.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void Resolve_DeclaredVariable()
    {
        var resolver = CreateResolver(out var table, out _);
        table.Declare(new VariableSymbol("x"));
        var result = resolver.ResolveIdentifier("x");
        result.Should().BeOfType<BoundVariableExpression>();
    }

    [Fact]
    public void ResolveFunction_KnownFunction()
    {
        var resolver = CreateResolver(out _, out _);
        var result = resolver.ResolveFunction("sin");
        result.Should().NotBeNull();
        result!.Name.Should().Be("sin");
    }

    [Fact]
    public void ResolveFunction_Undefined()
    {
        var resolver = CreateResolver(out _, out var diag);
        var result = resolver.ResolveFunction("unknown");
        result.Should().BeNull();
        diag.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void ResolveQualifiedName_StdG()
    {
        var resolver = CreateResolver(out _, out _);
        var result = resolver.ResolveQualifiedName("std.g");
        result.Should().BeOfType<BoundConstantExpression>();
    }

    [Fact]
    public void ResolveQualifiedName_UndefinedNamespace()
    {
        var resolver = CreateResolver(out _, out var diag);
        var result = resolver.ResolveQualifiedName("nope.x");
        result.Should().BeOfType<BoundLiteralExpression>();
        diag.HasErrors.Should().BeTrue();
    }
}
