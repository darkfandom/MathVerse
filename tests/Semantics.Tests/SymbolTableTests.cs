using FluentAssertions;
using MathVerse.Math.Semantics.Symbols;

namespace MathVerse.Semantics.Tests;

public class SymbolTableTests
{
    [Fact]
    public void SymbolTable_HasBuiltinsOnConstruction()
    {
        var table = new SymbolTable();
        table.Lookup("π").Should().NotBeNull();
        table.Lookup("e").Should().NotBeNull();
        table.Lookup("sin").Should().NotBeNull();
        table.Lookup("cos").Should().NotBeNull();
        table.Lookup("sqrt").Should().NotBeNull();
    }

    [Fact]
    public void SymbolTable_ConstantsAreConstant()
    {
        var table = new SymbolTable();
        table.IsConstant("π").Should().BeTrue();
        table.IsConstant("e").Should().BeTrue();
        table.IsConstant("sin").Should().BeFalse();
    }

    [Fact]
    public void SymbolTable_DeclareCustomSymbol()
    {
        var table = new SymbolTable();
        table.Declare(new VariableSymbol("myVar")).Should().BeTrue();
        table.Lookup("myVar").Should().NotBeNull();
    }

    [Fact]
    public void SymbolTable_StandardNamespaceExists()
    {
        var table = new SymbolTable();
        var ns = table.LookupGlobal("std");
        ns.Should().NotBeNull();
        ns.Should().BeOfType<NamespaceSymbol>();
    }

    [Fact]
    public void SymbolTable_ScopeManagement()
    {
        var table = new SymbolTable();
        table.EnterScope(ScopeKind.Function);
        table.Declare(new ParameterSymbol("x", 0));
        table.LookupLocal("x").Should().NotBeNull();
        table.ExitScope();
        table.LookupLocal("x").Should().BeNull();
    }

    [Fact]
    public void SymbolTable_DuplicateDeclaration_ReturnsFalse()
    {
        var table = new SymbolTable();
        table.Declare(new VariableSymbol("x")).Should().BeTrue();
        table.Declare(new VariableSymbol("x")).Should().BeFalse();
    }

    [Fact]
    public void SymbolTable_AllDeclared_TracksAll()
    {
        var table = new SymbolTable();
        int initialCount = table.AllDeclared.Count;
        table.Declare(new VariableSymbol("a"));
        table.Declare(new VariableSymbol("b"));
        table.AllDeclared.Count.Should().Be(initialCount + 2);
    }

    [Fact]
    public void SymbolTable_BuiltinFunctionCount()
    {
        var table = new SymbolTable();
        var sin = table.Lookup("sin");
        sin.Should().BeOfType<FunctionSymbol>();
        ((FunctionSymbol)sin!).ParameterCount.Should().Be(1);
    }

    [Fact]
    public void SymbolTable_MultiArgBuiltinFunction()
    {
        var table = new SymbolTable();
        var pow = table.Lookup("pow");
        pow.Should().BeOfType<FunctionSymbol>();
        ((FunctionSymbol)pow!).ParameterCount.Should().Be(2);
    }
}
