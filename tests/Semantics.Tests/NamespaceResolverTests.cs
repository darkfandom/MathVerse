using FluentAssertions;
using MathVerse.Math.Semantics;
using MathVerse.Math.Semantics.Binding;
using MathVerse.Math.Semantics.Diagnostics;
using MathVerse.Math.Semantics.Resolution;
using MathVerse.Math.Semantics.Symbols;

namespace MathVerse.Semantics.Tests;

public class NamespaceResolverTests
{
    private static NamespaceResolver CreateResolver(out SemanticDiagnosticBag diagnostics)
    {
        diagnostics = new SemanticDiagnosticBag();
        var table = new SymbolTable();
        var context = new BindingContext(table, diagnostics);
        return new NamespaceResolver(context);
    }

    [Fact]
    public void Resolve_StdG()
    {
        var resolver = CreateResolver(out _);
        var result = resolver.Resolve("std", "g");
        result.Should().NotBeNull();
        result!.Name.Should().Be("g");
    }

    [Fact]
    public void Resolve_StdC()
    {
        var resolver = CreateResolver(out _);
        var result = resolver.Resolve("std", "c");
        result.Should().NotBeNull();
    }

    [Fact]
    public void Resolve_UndefinedNamespace()
    {
        var resolver = CreateResolver(out var diag);
        var result = resolver.Resolve("nope", "x");
        result.Should().BeNull();
        diag.HasErrors.Should().BeTrue();
    }

    [Fact]
    public void ListMembers_Std()
    {
        var resolver = CreateResolver(out _);
        var members = resolver.ListMembers("std");
        members.Should().Contain("g");
        members.Should().Contain("c");
    }

    [Fact]
    public void ListMembers_Undefined()
    {
        var resolver = CreateResolver(out _);
        var members = resolver.ListMembers("nope");
        members.Should().BeEmpty();
    }
}
