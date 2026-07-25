using FluentAssertions;
using MathVerse.Math.Semantics.Symbols;

namespace MathVerse.Semantics.Tests;

public class ScopeStackTests
{
    [Fact]
    public void ScopeStack_StartsWithGlobalScope()
    {
        var stack = new ScopeStack();
        stack.CurrentScope.Should().Be(stack.GlobalScope);
        stack.Depth.Should().Be(1);
    }

    [Fact]
    public void ScopeStack_EnterScope_IncreasesDepth()
    {
        var stack = new ScopeStack();
        stack.EnterScope(ScopeKind.Function);
        stack.Depth.Should().Be(2);
    }

    [Fact]
    public void ScopeStack_ExitScope_DecreasesDepth()
    {
        var stack = new ScopeStack();
        stack.EnterScope(ScopeKind.Function);
        stack.ExitScope();
        stack.Depth.Should().Be(1);
    }

    [Fact]
    public void ScopeStack_ExitGlobalScope_Throws()
    {
        var stack = new ScopeStack();
        Action act = () => stack.ExitScope();
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ScopeStack_Declare_InCurrentScope()
    {
        var stack = new ScopeStack();
        stack.Declare(new VariableSymbol("x"));
        stack.LookupLocal("x").Should().NotBeNull();
    }

    [Fact]
    public void ScopeStack_Lookup_FindsInGlobal()
    {
        var stack = new ScopeStack();
        stack.Declare(new VariableSymbol("x"));
        stack.EnterScope(ScopeKind.Function);
        stack.Lookup("x").Should().NotBeNull();
        stack.ExitScope();
    }

    [Fact]
    public void ScopeStack_LookupGlobal_OnlyFindsGlobal()
    {
        var stack = new ScopeStack();
        stack.EnterScope(ScopeKind.Function);
        stack.Declare(new VariableSymbol("local"));
        stack.LookupGlobal("local").Should().BeNull();
        stack.ExitScope();
    }
}
