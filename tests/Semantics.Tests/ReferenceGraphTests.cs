using FluentAssertions;
using MathVerse.Math.Semantics.Symbols;
using MathVerse.Math.Semantics.Resolution;

namespace MathVerse.Semantics.Tests;

public class ReferenceGraphTests
{
    [Fact]
    public void AddReference_TracksSymbol()
    {
        var graph = new ReferenceGraph();
        var sym = new VariableSymbol("x");
        graph.AddReference(sym, "line 1");
        graph.Count.Should().Be(1);
    }

    [Fact]
    public void GetReferences_BySymbol()
    {
        var graph = new ReferenceGraph();
        var sym = new VariableSymbol("x");
        graph.AddReference(sym, "loc1");
        graph.AddReference(sym, "loc2");
        graph.GetReferences(sym).Should().HaveCount(2);
    }

    [Fact]
    public void GetReferences_ByName()
    {
        var graph = new ReferenceGraph();
        var sym = new VariableSymbol("x");
        graph.AddReference(sym, "loc1");
        graph.GetReferences("x").Should().HaveCount(1);
        graph.GetReferences("y").Should().HaveCount(0);
    }

    [Fact]
    public void IsReferenced_True()
    {
        var graph = new ReferenceGraph();
        var sym = new VariableSymbol("x");
        graph.AddReference(sym, null);
        graph.IsReferenced(sym).Should().BeTrue();
    }

    [Fact]
    public void IsReferenced_False()
    {
        var graph = new ReferenceGraph();
        var sym = new VariableSymbol("x");
        graph.IsReferenced(sym).Should().BeFalse();
    }

    [Fact]
    public void GetWriteReferences()
    {
        var graph = new ReferenceGraph();
        var sym = new VariableSymbol("x");
        graph.AddReference(sym, "read", isWrite: false);
        graph.AddReference(sym, "write", isWrite: true);
        graph.GetWriteReferences(sym).Should().HaveCount(1);
        graph.GetReadReferences(sym).Should().HaveCount(1);
    }

    [Fact]
    public void IsReadOnly_True()
    {
        var graph = new ReferenceGraph();
        var sym = new VariableSymbol("x");
        graph.AddReference(sym, "read", isWrite: false);
        graph.IsReadOnly(sym).Should().BeTrue();
    }

    [Fact]
    public void IsReadOnly_False()
    {
        var graph = new ReferenceGraph();
        var sym = new VariableSymbol("x");
        graph.AddReference(sym, "write", isWrite: true);
        graph.IsReadOnly(sym).Should().BeFalse();
    }

    [Fact]
    public void RecordDefinition()
    {
        var graph = new ReferenceGraph();
        var sym = new VariableSymbol("x");
        graph.RecordDefinition(sym, "line 10");
        graph.GetDefinitionSite("x").Should().Be("line 10");
    }

    [Fact]
    public void GetDefinitionSite_NotFound()
    {
        var graph = new ReferenceGraph();
        graph.GetDefinitionSite("missing").Should().BeNull();
    }

    [Fact]
    public void GetNeverWrittenSymbols()
    {
        var graph = new ReferenceGraph();
        var x = new VariableSymbol("x");
        var y = new VariableSymbol("y");
        graph.AddReference(x, null, isWrite: false);
        graph.AddReference(y, null, isWrite: true);
        graph.GetNeverWrittenSymbols().Should().Contain("x");
        graph.GetNeverWrittenSymbols().Should().NotContain("y");
    }

    [Fact]
    public void GetReferenceCount()
    {
        var graph = new ReferenceGraph();
        var sym = new VariableSymbol("x");
        graph.AddReference(sym, null);
        graph.AddReference(sym, null);
        graph.GetReferenceCount(sym).Should().Be(2);
    }
}
