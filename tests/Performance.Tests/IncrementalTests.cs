namespace MathVerse.Performance.Tests;

public sealed class IncrementalTests
{
    [Fact]
    public void DependencyNode_InitialState()
    {
        var node = new DependencyNode(1, "test");

        node.Id.Should().Be(1);
        node.Name.Should().Be("test");
        node.IsDirty.Should().BeFalse();
        node.Dependents.Should().BeEmpty();
        node.Dependencies.Should().BeEmpty();
    }

    [Fact]
    public void DependencyNode_MarkDirty()
    {
        var node = new DependencyNode(1, "test");

        node.MarkDirty();

        node.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void DependencyNode_MarkClean()
    {
        var node = new DependencyNode(1, "test");

        node.MarkDirty();
        node.MarkClean();

        node.IsDirty.Should().BeFalse();
    }

    [Fact]
    public void DependencyNode_NullName_Throws()
    {
        Action act = () => new DependencyNode(1, null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DependencyNode_ToString()
    {
        var node = new DependencyNode(42, "myNode");

        var str = node.ToString();

        str.Should().Contain("Id=42");
        str.Should().Contain("Name=myNode");
    }

    [Fact]
    public void DependencyNode_DependenciesTracking()
    {
        var tracker = new DependencyTracker();
        var a = tracker.AddNode("A");
        var b = tracker.AddNode("B");

        tracker.AddDependency(a, b);

        var nodeA = tracker.GetNode(a)!;
        nodeA.Dependencies.Should().Contain(b);
    }

    [Fact]
    public void DependencyNode_DependentsTracking()
    {
        var tracker = new DependencyTracker();
        var a = tracker.AddNode("A");
        var b = tracker.AddNode("B");

        tracker.AddDependency(a, b);

        var nodeB = tracker.GetNode(b)!;
        nodeB.Dependents.Should().Contain(a);
    }

    [Fact]
    public void DependencyTracker_AddNode()
    {
        var tracker = new DependencyTracker();

        var id = tracker.AddNode("test");

        id.Should().BeGreaterThan(0);
        tracker.NodeCount.Should().Be(1);
    }

    [Fact]
    public void DependencyTracker_MultipleNodes()
    {
        var tracker = new DependencyTracker();

        var a = tracker.AddNode("A");
        var b = tracker.AddNode("B");
        var c = tracker.AddNode("C");

        tracker.NodeCount.Should().Be(3);
    }

    [Fact]
    public void DependencyTracker_GetNode()
    {
        var tracker = new DependencyTracker();
        var id = tracker.AddNode("test");

        var node = tracker.GetNode(id);

        node.Should().NotBeNull();
        node!.Name.Should().Be("test");
    }

    [Fact]
    public void DependencyTracker_GetNode_NotFound()
    {
        var tracker = new DependencyTracker();

        tracker.GetNode(999).Should().BeNull();
    }

    [Fact]
    public void DependencyTracker_AddDependency()
    {
        var tracker = new DependencyTracker();
        var a = tracker.AddNode("A");
        var b = tracker.AddNode("B");

        tracker.AddDependency(a, b);

        var nodeA = tracker.GetNode(a)!;
        nodeA.Dependencies.Should().HaveCount(1);
        nodeA.Dependencies.Should().Contain(b);

        var nodeB = tracker.GetNode(b)!;
        nodeB.Dependents.Should().HaveCount(1);
        nodeB.Dependents.Should().Contain(a);
    }

    [Fact]
    public void DependencyTracker_AddDependency_NodeNotFound_Throws()
    {
        var tracker = new DependencyTracker();
        var a = tracker.AddNode("A");

        Action act = () => tracker.AddDependency(a, 999);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DependencyTracker_AddDependency_DependentNotFound_Throws()
    {
        var tracker = new DependencyTracker();
        var b = tracker.AddNode("B");

        Action act = () => tracker.AddDependency(999, b);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void DependencyTracker_MarkDirty_NoDependents()
    {
        var tracker = new DependencyTracker();
        var a = tracker.AddNode("A");

        tracker.MarkDirty(a);

        tracker.GetNode(a)!.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void DependencyTracker_MarkDirty_Transitive()
    {
        var tracker = new DependencyTracker();
        var a = tracker.AddNode("A");
        var b = tracker.AddNode("B");
        var c = tracker.AddNode("C");

        tracker.AddDependency(b, a);
        tracker.AddDependency(c, b);

        tracker.MarkDirty(a);

        tracker.GetNode(a)!.IsDirty.Should().BeTrue();
        tracker.GetNode(b)!.IsDirty.Should().BeTrue();
        tracker.GetNode(c)!.IsDirty.Should().BeTrue();
    }

    [Fact]
    public void DependencyTracker_GetDirtyNodes()
    {
        var tracker = new DependencyTracker();
        var a = tracker.AddNode("A");
        var b = tracker.AddNode("B");
        var c = tracker.AddNode("C");

        tracker.MarkDirty(a);
        tracker.MarkDirty(c);

        var dirty = tracker.GetDirtyNodes();

        dirty.Should().Contain(a);
        dirty.Should().NotContain(b);
        dirty.Should().Contain(c);
    }

    [Fact]
    public void DependencyTracker_GetDirtyNodes_Empty()
    {
        var tracker = new DependencyTracker();

        tracker.GetDirtyNodes().Should().BeEmpty();
    }

    [Fact]
    public void DependencyTracker_MarkAllClean()
    {
        var tracker = new DependencyTracker();
        var a = tracker.AddNode("A");
        var b = tracker.AddNode("B");

        tracker.MarkDirty(a);
        tracker.MarkDirty(b);

        tracker.MarkAllClean();

        tracker.GetDirtyNodes().Should().BeEmpty();
    }

    [Fact]
    public void DependencyTracker_RemoveNode()
    {
        var tracker = new DependencyTracker();
        var a = tracker.AddNode("A");
        var b = tracker.AddNode("B");
        tracker.AddDependency(b, a);

        tracker.RemoveNode(a);

        tracker.GetNode(a).Should().BeNull();
        tracker.GetNode(b)!.Dependents.Should().BeEmpty();
    }

    [Fact]
    public void DependencyTracker_RemoveNode_NonExistent()
    {
        var tracker = new DependencyTracker();

        Action act = () => tracker.RemoveNode(999);

        act.Should().NotThrow();
    }

    [Fact]
    public void DependencyTracker_RemoveNode_CleansDependents()
    {
        var tracker = new DependencyTracker();
        var a = tracker.AddNode("A");
        var b = tracker.AddNode("B");
        var c = tracker.AddNode("C");

        tracker.AddDependency(b, a);
        tracker.AddDependency(c, a);

        tracker.RemoveNode(a);

        tracker.GetNode(b)!.Dependents.Should().BeEmpty();
        tracker.GetNode(c)!.Dependents.Should().BeEmpty();
    }

    [Fact]
    public void DependencyTracker_MultipleDependencies()
    {
        var tracker = new DependencyTracker();
        var a = tracker.AddNode("A");
        var b = tracker.AddNode("B");
        var c = tracker.AddNode("C");

        tracker.AddDependency(a, b);
        tracker.AddDependency(a, c);

        var nodeA = tracker.GetNode(a)!;
        nodeA.Dependencies.Should().HaveCount(2);
    }

    [Fact]
    public void DependencyTracker_NoDuplicateDependencies()
    {
        var tracker = new DependencyTracker();
        var a = tracker.AddNode("A");
        var b = tracker.AddNode("B");

        tracker.AddDependency(a, b);
        tracker.AddDependency(a, b);

        tracker.GetNode(a)!.Dependencies.Should().HaveCount(1);
        tracker.GetNode(b)!.Dependents.Should().HaveCount(1);
    }

    [Fact]
    public void ChangeSet_Empty()
    {
        var changeSet = ChangeSet.Empty;

        changeSet.HasChanges.Should().BeFalse();
        changeSet.ChangedNodes.Should().BeEmpty();
        changeSet.AffectedNodes.Should().BeEmpty();
    }

    [Fact]
    public void ChangeSet_WithChanges()
    {
        var changed = new HashSet<int> { 1, 2 };
        var affected = new HashSet<int> { 1, 2, 3, 4 };
        var changeSet = new ChangeSet(changed, affected);

        changeSet.HasChanges.Should().BeTrue();
        changeSet.ChangedNodes.Should().HaveCount(2);
        changeSet.AffectedNodes.Should().HaveCount(4);
    }

    [Fact]
    public void ChangeSet_Merge()
    {
        var a = new ChangeSet(new HashSet<int> { 1 }, new HashSet<int> { 1, 2 });
        var b = new ChangeSet(new HashSet<int> { 3 }, new HashSet<int> { 3, 4 });

        var merged = a.Merge(b);

        merged.ChangedNodes.Should().HaveCount(2);
        merged.AffectedNodes.Should().HaveCount(4);
    }

    [Fact]
    public void ChangeSet_MergeOverlapping()
    {
        var a = new ChangeSet(new HashSet<int> { 1 }, new HashSet<int> { 1, 2 });
        var b = new ChangeSet(new HashSet<int> { 2 }, new HashSet<int> { 2, 3 });

        var merged = a.Merge(b);

        merged.ChangedNodes.Should().HaveCount(2);
        merged.AffectedNodes.Should().HaveCount(3);
    }

    [Fact]
    public void ChangeSet_MergeNull_Throws()
    {
        var a = ChangeSet.Empty;

        Action act = () => a.Merge(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ChangeSet_OriginalNotModified()
    {
        var changed = new HashSet<int> { 1 };
        var affected = new HashSet<int> { 1 };
        var a = new ChangeSet(changed, affected);
        var b = new ChangeSet(new HashSet<int> { 2 }, new HashSet<int> { 2 });

        a.Merge(b);

        a.ChangedNodes.Should().HaveCount(1);
    }

    [Fact]
    public void InvalidationGraph_SetTracker()
    {
        var graph = new InvalidationGraph();
        var tracker = new DependencyTracker();

        Action act = () => graph.SetTracker(tracker);

        act.Should().NotThrow();
    }

    [Fact]
    public void InvalidationGraph_SetTrackerNull_Throws()
    {
        var graph = new InvalidationGraph();

        Action act = () => graph.SetTracker(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void InvalidationGraph_Propagate_NoTracker_Throws()
    {
        var graph = new InvalidationGraph();

        Action act = () => graph.Propagate(ChangeSet.Empty);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void InvalidationGraph_Propagate_NoChanges()
    {
        var tracker = new DependencyTracker();
        var graph = new InvalidationGraph();
        graph.SetTracker(tracker);

        var result = graph.Propagate(ChangeSet.Empty);

        result.AffectedNodes.Should().BeEmpty();
    }

    [Fact]
    public void InvalidationGraph_Propagate_SingleNode()
    {
        var tracker = new DependencyTracker();
        var a = tracker.AddNode("A");
        var graph = new InvalidationGraph();
        graph.SetTracker(tracker);

        var changes = new ChangeSet(new HashSet<int> { a }, new HashSet<int> { a });
        var result = graph.Propagate(changes);

        result.AffectedNodes.Should().Contain(a);
    }

    [Fact]
    public void InvalidationGraph_Propagate_Transitive()
    {
        var tracker = new DependencyTracker();
        var a = tracker.AddNode("A");
        var b = tracker.AddNode("B");
        var c = tracker.AddNode("C");

        tracker.AddDependency(b, a);
        tracker.AddDependency(c, b);

        var graph = new InvalidationGraph();
        graph.SetTracker(tracker);

        var changes = new ChangeSet(new HashSet<int> { a }, new HashSet<int> { a });
        var result = graph.Propagate(changes);

        result.AffectedNodes.Should().Contain(a);
        result.AffectedNodes.Should().Contain(b);
        result.AffectedNodes.Should().Contain(c);
    }

    [Fact]
    public void InvalidationGraph_Propagate_DiamondDependency()
    {
        var tracker = new DependencyTracker();
        var root = tracker.AddNode("Root");
        var left = tracker.AddNode("Left");
        var right = tracker.AddNode("Right");
        var bottom = tracker.AddNode("Bottom");

        tracker.AddDependency(left, root);
        tracker.AddDependency(right, root);
        tracker.AddDependency(bottom, left);
        tracker.AddDependency(bottom, right);

        var graph = new InvalidationGraph();
        graph.SetTracker(tracker);

        var changes = new ChangeSet(new HashSet<int> { root }, new HashSet<int> { root });
        var result = graph.Propagate(changes);

        result.AffectedNodes.Should().HaveCount(4);
    }

    [Fact]
    public void InvalidationGraph_PropagateNull_Throws()
    {
        var tracker = new DependencyTracker();
        var graph = new InvalidationGraph();
        graph.SetTracker(tracker);

        Action act = () => graph.Propagate(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void InvalidationGraph_Propagate_NoDependents()
    {
        var tracker = new DependencyTracker();
        var a = tracker.AddNode("A");
        var b = tracker.AddNode("B");

        var graph = new InvalidationGraph();
        graph.SetTracker(tracker);

        var changes = new ChangeSet(new HashSet<int> { a }, new HashSet<int> { a });
        var result = graph.Propagate(changes);

        result.AffectedNodes.Should().HaveCount(1);
        result.AffectedNodes.Should().Contain(a);
        result.AffectedNodes.Should().NotContain(b);
    }

    [Fact]
    public void IncrementalEvaluator_Evaluate_Literal()
    {
        var evaluator = new IncrementalEvaluator();
        var expr = Expr.Literal(42.0);

        var result = evaluator.Evaluate(expr);

        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(42.0);
    }

    [Fact]
    public void IncrementalEvaluator_Evaluate_Variable()
    {
        var evaluator = new IncrementalEvaluator();
        var expr = Expr.Variable("x");

        var result = evaluator.Evaluate(expr);

        result.Should().BeOfType<VariableExpression>();
    }

    [Fact]
    public void IncrementalEvaluator_Evaluate_ConstantFolding()
    {
        var evaluator = new IncrementalEvaluator();
        var expr = Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0));

        var result = evaluator.Evaluate(expr);

        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(3.0);
    }

    [Fact]
    public void IncrementalEvaluator_Evaluate_MultiplyFold()
    {
        var evaluator = new IncrementalEvaluator();
        var expr = Expr.Multiply(Expr.Literal(3.0), Expr.Literal(4.0));

        var result = evaluator.Evaluate(expr);

        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(12.0);
    }

    [Fact]
    public void IncrementalEvaluator_Evaluate_SubtractFold()
    {
        var evaluator = new IncrementalEvaluator();
        var expr = Expr.Subtract(Expr.Literal(10.0), Expr.Literal(3.0));

        var result = evaluator.Evaluate(expr);

        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(7.0);
    }

    [Fact]
    public void IncrementalEvaluator_Evaluate_DivideFold()
    {
        var evaluator = new IncrementalEvaluator();
        var expr = Expr.Divide(Expr.Literal(10.0), Expr.Literal(2.0));

        var result = evaluator.Evaluate(expr);

        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(5.0);
    }

    [Fact]
    public void IncrementalEvaluator_Evaluate_PowerFold()
    {
        var evaluator = new IncrementalEvaluator();
        var expr = Expr.Pow(Expr.Literal(2.0), Expr.Literal(3.0));

        var result = evaluator.Evaluate(expr);

        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(8.0);
    }

    [Fact]
    public void IncrementalEvaluator_Evaluate_ModuloFold()
    {
        var evaluator = new IncrementalEvaluator();
        var expr = Expr.Modulo(Expr.Literal(10.0), Expr.Literal(3.0));

        var result = evaluator.Evaluate(expr);

        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(1.0);
    }

    [Fact]
    public void IncrementalEvaluator_Evaluate_AddZero()
    {
        var evaluator = new IncrementalEvaluator();
        var x = Expr.Variable("x");
        var expr = Expr.Add(x, Expr.Literal(0.0));

        var result = evaluator.Evaluate(expr);

        result.Should().Be(x);
    }

    [Fact]
    public void IncrementalEvaluator_Evaluate_ZeroAdd()
    {
        var evaluator = new IncrementalEvaluator();
        var x = Expr.Variable("x");
        var expr = Expr.Add(Expr.Literal(0.0), x);

        var result = evaluator.Evaluate(expr);

        result.Should().Be(x);
    }

    [Fact]
    public void IncrementalEvaluator_Evaluate_MultiplyByOne()
    {
        var evaluator = new IncrementalEvaluator();
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(x, Expr.Literal(1.0));

        var result = evaluator.Evaluate(expr);

        result.Should().Be(x);
    }

    [Fact]
    public void IncrementalEvaluator_Evaluate_OneMultiply()
    {
        var evaluator = new IncrementalEvaluator();
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(Expr.Literal(1.0), x);

        var result = evaluator.Evaluate(expr);

        result.Should().Be(x);
    }

    [Fact]
    public void IncrementalEvaluator_Evaluate_MultiplyByZero()
    {
        var evaluator = new IncrementalEvaluator();
        var x = Expr.Variable("x");
        var expr = Expr.Multiply(x, Expr.Literal(0.0));

        var result = evaluator.Evaluate(expr);

        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(0.0);
    }

    [Fact]
    public void IncrementalEvaluator_Evaluate_NegateLiteral()
    {
        var evaluator = new IncrementalEvaluator();
        var expr = Expr.Negate(Expr.Literal(5.0));

        var result = evaluator.Evaluate(expr);

        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(-5.0);
    }

    [Fact]
    public void IncrementalEvaluator_Evaluate_NonSimplifiableBinary()
    {
        var evaluator = new IncrementalEvaluator();
        var expr = Expr.Add(Expr.Variable("x"), Expr.Variable("y"));

        var result = evaluator.Evaluate(expr);

        result.Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public void IncrementalEvaluator_CachesResults()
    {
        var evaluator = new IncrementalEvaluator();
        var expr = Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0));

        var r1 = evaluator.Evaluate(expr);
        var r2 = evaluator.Evaluate(expr);

        evaluator.CacheSize.Should().BeGreaterThan(0);
    }

    [Fact]
    public void IncrementalEvaluator_Invalidate()
    {
        var evaluator = new IncrementalEvaluator();
        var expr = Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0));

        evaluator.Evaluate(expr);
        evaluator.Invalidate(expr);

        evaluator.CacheSize.Should().Be(0);
    }

    [Fact]
    public void IncrementalEvaluator_InvalidateAll()
    {
        var evaluator = new IncrementalEvaluator();
        evaluator.Evaluate(Expr.Literal(1.0));
        evaluator.Evaluate(Expr.Literal(2.0));

        evaluator.InvalidateAll();

        evaluator.CacheSize.Should().Be(0);
    }

    [Fact]
    public void IncrementalEvaluator_InvalidateNull_Throws()
    {
        var evaluator = new IncrementalEvaluator();
        Action act = () => evaluator.Invalidate(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IncrementalEvaluator_EvaluateNull_Throws()
    {
        var evaluator = new IncrementalEvaluator();
        Action act = () => evaluator.Evaluate(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IncrementalEngine_Evaluate()
    {
        var engine = new IncrementalEngine();
        var expr = Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0));

        var result = engine.Evaluate(expr);

        result.Should().BeOfType<LiteralExpression>();
        ((LiteralExpression)result).Value.Should().Be(3.0);
    }

    [Fact]
    public void IncrementalEngine_EvaluateCaches()
    {
        var engine = new IncrementalEngine();
        var expr = Expr.Literal(42.0);

        var r1 = engine.Evaluate(expr);
        var r2 = engine.Evaluate(expr);

        r1.Should().Be(r2);
    }

    [Fact]
    public void IncrementalEngine_Invalidate()
    {
        var engine = new IncrementalEngine();
        var expr = Expr.Literal(42.0);

        engine.Evaluate(expr);
        engine.Invalidate(expr);

        var result = engine.Evaluate(expr);
        result.Should().BeOfType<LiteralExpression>();
    }

    [Fact]
    public void IncrementalEngine_InvalidateNull_Throws()
    {
        var engine = new IncrementalEngine();
        Action act = () => engine.Invalidate(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IncrementalEngine_EvaluateNull_Throws()
    {
        var engine = new IncrementalEngine();
        Action act = () => engine.Evaluate(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void IncrementalEngine_Reset()
    {
        var engine = new IncrementalEngine();
        engine.Evaluate(Expr.Literal(1.0));
        engine.Evaluate(Expr.Literal(2.0));

        engine.Reset();

        engine.Evaluate(Expr.Literal(1.0));
    }

    [Fact]
    public void IncrementalEngine_Update_NoDirtyNodes()
    {
        var engine = new IncrementalEngine();

        var changeSet = engine.Update();

        changeSet.HasChanges.Should().BeFalse();
    }

    [Fact]
    public void IncrementalEngine_Dependencies()
    {
        var engine = new IncrementalEngine();

        engine.Dependencies.Should().NotBeNull();
    }

    [Fact]
    public void IncrementalEngine_MultipleEvaluations()
    {
        var engine = new IncrementalEngine();
        var expressions = new Expression[]
        {
            Expr.Literal(1.0),
            Expr.Add(Expr.Literal(1.0), Expr.Literal(2.0)),
            Expr.Multiply(Expr.Literal(3.0), Expr.Literal(4.0)),
            Expr.Pow(Expr.Literal(2.0), Expr.Literal(10.0)),
        };

        foreach (var expr in expressions)
        {
            var result = engine.Evaluate(expr);
            result.Should().NotBeNull();
        }
    }

    [Fact]
    public void IncrementalEngine_ComplexExpression()
    {
        var engine = new IncrementalEngine();
        var expr = Expr.Add(
            Expr.Multiply(Expr.Literal(2.0), Expr.Literal(3.0)),
            Expr.Pow(Expr.Literal(4.0), Expr.Literal(2.0)));

        var result = engine.Evaluate(expr);

        result.Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public async Task DependencyTracker_ThreadSafety()
    {
        var tracker = new DependencyTracker();
        var tasks = Enumerable.Range(0, 50)
            .Select(i => Task.Run(() =>
            {
                var id = tracker.AddNode($"Node{i}");
                tracker.MarkDirty(id);
            }))
            .ToArray();

        await Task.WhenAll(tasks);
        tracker.NodeCount.Should().Be(50);
    }

    [Fact]
    public void DependencyTracker_MarkDirty_NonExistentNode()
    {
        var tracker = new DependencyTracker();

        Action act = () => tracker.MarkDirty(999);

        act.Should().NotThrow();
    }

    [Fact]
    public void ChangeSet_ChangedNodesContainsOriginal()
    {
        var changed = new HashSet<int> { 1, 2 };
        var affected = new HashSet<int> { 1, 2, 3 };
        var cs = new ChangeSet(changed, affected);

        cs.ChangedNodes.Should().BeSubsetOf(cs.AffectedNodes);
    }

    [Fact]
    public void IncrementalEvaluator_InvalidNonExistent()
    {
        var evaluator = new IncrementalEvaluator();
        var expr = Expr.Literal(42.0);

        Action act = () => evaluator.Invalidate(expr);

        act.Should().NotThrow();
    }

    [Fact]
    public void IncrementalEngine_ConsistentResults()
    {
        var engine = new IncrementalEngine();

        var r1 = engine.Evaluate(Expr.Add(Expr.Literal(2.0), Expr.Literal(3.0)));
        var r2 = engine.Evaluate(Expr.Add(Expr.Literal(2.0), Expr.Literal(3.0)));

        r1.Should().Be(r2);
    }

    [Fact]
    public void IncrementalEvaluator_NegateNonLiteral()
    {
        var evaluator = new IncrementalEvaluator();
        var expr = Expr.Negate(Expr.Variable("x"));

        var result = evaluator.Evaluate(expr);

        result.Should().BeOfType<UnaryExpression>();
    }

    [Fact]
    public void IncrementalEvaluator_BinaryWithVariable()
    {
        var evaluator = new IncrementalEvaluator();
        var expr = Expr.Add(Expr.Variable("x"), Expr.Literal(1.0));

        var result = evaluator.Evaluate(expr);

        result.Should().BeOfType<BinaryExpression>();
    }

    [Fact]
    public async Task IncrementalEngine_ThreadSafety()
    {
        var engine = new IncrementalEngine();
        var tasks = Enumerable.Range(0, 50)
            .Select(i => Task.Run(() =>
            {
                var expr = Expr.Literal(i);
                engine.Evaluate(expr);
            }))
            .ToArray();

        await Task.WhenAll(tasks);
    }

    [Fact]
    public void IncrementalEvaluator_MultipleCaches()
    {
        var evaluator = new IncrementalEvaluator();
        evaluator.Evaluate(Expr.Literal(1.0));
        evaluator.Evaluate(Expr.Literal(2.0));
        evaluator.Evaluate(Expr.Literal(3.0));

        evaluator.CacheSize.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public void IncrementalEngine_EvaluateThenInvalidate()
    {
        var engine = new IncrementalEngine();
        var expr = Expr.Add(Expr.Literal(5.0), Expr.Literal(5.0));

        var r1 = engine.Evaluate(expr);
        ((LiteralExpression)r1).Value.Should().Be(10.0);

        engine.Invalidate(expr);

        var r2 = engine.Evaluate(expr);
        ((LiteralExpression)r2).Value.Should().Be(10.0);
    }

    [Fact]
    public void IncrementalEngine_UpdateAfterDependencyChange()
    {
        var engine = new IncrementalEngine();
        var id1 = engine.Dependencies.AddNode("A");
        var id2 = engine.Dependencies.AddNode("B");
        engine.Dependencies.AddDependency(id2, id1);

        engine.Dependencies.MarkDirty(id1);

        var changeSet = engine.Update();

        changeSet.HasChanges.Should().BeTrue();
        changeSet.AffectedNodes.Should().Contain(id1);
        changeSet.AffectedNodes.Should().Contain(id2);
    }

    [Fact]
    public void IncrementalEngine_UpdateMarksNodesClean()
    {
        var engine = new IncrementalEngine();
        var id = engine.Dependencies.AddNode("A");
        engine.Dependencies.MarkDirty(id);

        engine.Update();

        engine.Dependencies.GetNode(id)!.IsDirty.Should().BeFalse();
    }
}
