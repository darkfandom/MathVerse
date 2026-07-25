using FluentAssertions;
using MathVerse.Math.Expressions;
using MathVerse.Math.Semantics;
using MathVerse.Math.Semantics.Binding;
using MathVerse.Math.Semantics.Builtins;
using MathVerse.Math.Semantics.Diagnostics;
using MathVerse.Math.Semantics.Resolution;
using MathVerse.Math.Semantics.Symbols;

namespace MathVerse.Semantics.Tests;

public class SymbolTests
{
    [Fact]
    public void VariableSymbol_HasCorrectProperties()
    {
        var sym = new VariableSymbol("x");
        sym.Name.Should().Be("x");
        sym.Kind.Should().Be(SymbolKind.Variable);
        sym.IsMutable.Should().BeTrue();
    }

    [Fact]
    public void VariableSymbol_Immutable()
    {
        var sym = new VariableSymbol("c", isMutable: false);
        sym.IsMutable.Should().BeFalse();
    }

    [Fact]
    public void FunctionSymbol_HasParameters()
    {
        var p0 = new ParameterSymbol("a", 0);
        var p1 = new ParameterSymbol("b", 1);
        var func = new FunctionSymbol("f", [p0, p1]);
        func.Name.Should().Be("f");
        func.Kind.Should().Be(SymbolKind.Function);
        func.ParameterCount.Should().Be(2);
        func.Parameters.Should().HaveCount(2);
        func.Body.Should().BeNull();
    }

    [Fact]
    public void FunctionSymbol_WithBody()
    {
        var body = Expr.Literal(42.0);
        var func = new FunctionSymbol("f", [new ParameterSymbol("x", 0)], body);
        func.Body.Should().Be(body);
    }

    [Fact]
    public void ConstantSymbol_HasValue()
    {
        var sym = new ConstantSymbol("pi", 3.14159);
        sym.Value.Should().Be(3.14159);
        sym.Kind.Should().Be(SymbolKind.Constant);
    }

    [Fact]
    public void ParameterSymbol_HasOrdinal()
    {
        var sym = new ParameterSymbol("x", 3);
        sym.Ordinal.Should().Be(3);
        sym.Kind.Should().Be(SymbolKind.Parameter);
    }

    [Fact]
    public void NamespaceSymbol_CanDeclareMembers()
    {
        var ns = new NamespaceSymbol("std");
        ns.Declare(new ConstantSymbol("g", 9.81));
        ns.Members.Should().ContainKey("g");
    }

    [Fact]
    public void Symbol_Equality()
    {
        var a = new VariableSymbol("x");
        var b = new VariableSymbol("x");
        a.Equals(b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void Symbol_Inequality()
    {
        var a = new VariableSymbol("x");
        var b = new VariableSymbol("y");
        a.Equals(b).Should().BeFalse();
    }

    [Fact]
    public void TypeSymbol_HasName()
    {
        var sym = new TypeSymbol("Real");
        sym.Name.Should().Be("Real");
        sym.Kind.Should().Be(SymbolKind.Type);
    }
}
