using FluentAssertions;
using MathVerse.Math.Semantics.Symbols;

namespace MathVerse.Semantics.Tests;

public class ScopeTests
{
    [Fact]
    public void SymbolScope_DeclareAndLookupLocal()
    {
        var scope = new SymbolScope(ScopeKind.Global);
        var sym = new VariableSymbol("x");
        scope.Declare(sym).Should().BeTrue();
        scope.LookupLocal("x").Should().Be(sym);
    }

    [Fact]
    public void SymbolScope_DuplicateDeclare_ReturnsFalse()
    {
        var scope = new SymbolScope(ScopeKind.Global);
        scope.Declare(new VariableSymbol("x")).Should().BeTrue();
        scope.Declare(new VariableSymbol("x")).Should().BeFalse();
    }

    [Fact]
    public void SymbolScope_LookupLocal_ReturnsNullForMissing()
    {
        var scope = new SymbolScope(ScopeKind.Global);
        scope.LookupLocal("missing").Should().BeNull();
    }

    [Fact]
    public void SymbolScope_Lookup_FindsInParent()
    {
        var parent = new SymbolScope(ScopeKind.Global);
        parent.Declare(new VariableSymbol("x"));

        var child = new SymbolScope(ScopeKind.Function, parent);
        child.Lookup("x").Should().NotBeNull();
        child.Lookup("x")!.Name.Should().Be("x");
    }

    [Fact]
    public void SymbolScope_Lookup_ReturnsNullWhenNotInChain()
    {
        var parent = new SymbolScope(ScopeKind.Global);
        var child = new SymbolScope(ScopeKind.Function, parent);
        child.Lookup("missing").Should().BeNull();
    }

    [Fact]
    public void SymbolScope_LocalShadowParent()
    {
        var parent = new SymbolScope(ScopeKind.Global);
        parent.Declare(new VariableSymbol("x"));

        var child = new SymbolScope(ScopeKind.Function, parent);
        var localX = new VariableSymbol("x");
        child.Declare(localX);

        child.Lookup("x").Should().Be(localX);
    }

    [Fact]
    public void SymbolScope_ContainsLocal()
    {
        var scope = new SymbolScope(ScopeKind.Global);
        scope.Declare(new VariableSymbol("x"));
        scope.ContainsLocal("x").Should().BeTrue();
        scope.ContainsLocal("y").Should().BeFalse();
    }
}
