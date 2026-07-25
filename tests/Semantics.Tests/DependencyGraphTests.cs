using FluentAssertions;
using MathVerse.Math.Semantics.Resolution;

namespace MathVerse.Semantics.Tests;

public class DependencyGraphTests
{
    [Fact]
    public void HasCycles_NoDependencies()
    {
        var graph = new DependencyGraph();
        graph.HasCycles().Should().BeFalse();
    }

    [Fact]
    public void HasCycles_NoCycle()
    {
        var graph = new DependencyGraph();
        graph.AddDependency("a", "b");
        graph.AddDependency("b", "c");
        graph.HasCycles().Should().BeFalse();
    }

    [Fact]
    public void HasCycles_SelfCycle()
    {
        var graph = new DependencyGraph();
        graph.AddDependency("a", "a");
        graph.HasCycles().Should().BeTrue();
    }

    [Fact]
    public void HasCycles_TwoNodeCycle()
    {
        var graph = new DependencyGraph();
        graph.AddDependency("a", "b");
        graph.AddDependency("b", "a");
        graph.HasCycles().Should().BeTrue();
    }

    [Fact]
    public void GetDependencies()
    {
        var graph = new DependencyGraph();
        graph.AddDependency("a", "b");
        graph.AddDependency("a", "c");
        graph.GetDependencies("a").Should().HaveCount(2);
        graph.GetDependencies("a").Should().Contain("b");
        graph.GetDependencies("a").Should().Contain("c");
    }

    [Fact]
    public void GetDependencies_Empty()
    {
        var graph = new DependencyGraph();
        graph.GetDependencies("x").Should().BeEmpty();
    }

    [Fact]
    public void GetTransitiveDependencies()
    {
        var graph = new DependencyGraph();
        graph.AddDependency("a", "b");
        graph.AddDependency("b", "c");
        var trans = graph.GetTransitiveDependencies("a");
        trans.Should().Contain("b");
        trans.Should().Contain("c");
    }

    [Fact]
    public void GetLeafNodes()
    {
        var graph = new DependencyGraph();
        graph.AddDependency("a", "b");
        var leaves = graph.GetLeafNodes();
        leaves.Should().Contain("b");
        leaves.Should().NotContain("a");
    }

    [Fact]
    public void FindCycles_ReturnsCyclePaths()
    {
        var graph = new DependencyGraph();
        graph.AddDependency("a", "b");
        graph.AddDependency("b", "c");
        graph.AddDependency("c", "a");
        var cycles = graph.FindCycles();
        cycles.Should().NotBeEmpty();
    }
}
