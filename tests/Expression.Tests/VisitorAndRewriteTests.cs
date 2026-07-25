namespace MathVerse.Expression.Tests;

public class ExpressionClonerTests
{
    [Fact]
    public void Clone_Literal_ReturnsEqualButNotSameReference()
    {
        var original = Expr.Literal(5);

        var clone = ExpressionCloner.Clone(original);

        clone.Should().Be(original);
        clone.Should().NotBeSameAs(original);
    }

    [Fact]
    public void Clone_Variable_PreservesName()
    {
        var original = Expr.Variable("x");

        var clone = ExpressionCloner.Clone(original);

        clone.Should().Be(original);
        ((VariableExpression)clone).Name.Should().Be("x");
    }

    [Fact]
    public void Clone_ComplexTree_ProducesDeepClone()
    {
        var x = Expr.Variable("x");
        var two = Expr.Literal(2);
        var xSquared = Expr.Pow(x, two);
        var three = Expr.Literal(3);
        var expr = Expr.Add(xSquared, three);

        var clone = ExpressionCloner.Clone(expr);

        clone.Should().Be(expr);
        clone.Should().NotBeSameAs(expr);
        ((BinaryExpression)clone).Left.Should().NotBeSameAs(((BinaryExpression)expr).Left);
        ((BinaryExpression)clone).Right.Should().NotBeSameAs(((BinaryExpression)expr).Right);
    }

    [Fact]
    public void Clone_NullExpression_ReturnsSameSingleton()
    {
        var original = Expr.Null;

        var clone = ExpressionCloner.Clone(original);

        clone.Should().Be(original);
        clone.Should().BeSameAs(NullExpression.Instance);
    }

    [Fact]
    public void Clone_Vector_ClonesAllComponents()
    {
        var original = Expr.Vector(Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));

        var clone = ExpressionCloner.Clone(original);

        clone.Should().Be(original);
        clone.Should().NotBeSameAs(original);
        var clonedVec = (VectorExpression)clone;
        clonedVec.Components.Should().HaveCount(3);
        clonedVec.Components[0].Should().Be(Expr.Literal(1));
    }
}

public class ExpressionPrinterTests
{
    [Fact]
    public void Print_Literal5_ReturnsDigits()
    {
        var expr = Expr.Literal(5);
        ExpressionPrinter.Print(expr).Should().Be("5");
    }

    [Fact]
    public void Print_LiteralPi_ReturnsFormattedDecimal()
    {
        var expr = Expr.Literal(3.14);
        ExpressionPrinter.Print(expr).Should().Be("3.14");
    }

    [Fact]
    public void Print_Variable_ReturnsName()
    {
        ExpressionPrinter.Print(Expr.Variable("x")).Should().Be("x");
    }

    [Fact]
    public void Print_Add_ReturnsParenthesizedExpression()
    {
        var expr = Expr.Add(Expr.Variable("x"), Expr.Variable("y"));
        ExpressionPrinter.Print(expr).Should().Be("(x + y)");
    }

    [Fact]
    public void Print_Negate_ReturnsMinusPrefix()
    {
        var expr = Expr.Negate(Expr.Variable("x"));
        ExpressionPrinter.Print(expr).Should().Be("-x");
    }

    [Fact]
    public void Print_Sin_ReturnsFunctionSyntax()
    {
        var expr = Expr.Sin(Expr.Variable("x"));
        ExpressionPrinter.Print(expr).Should().Be("sin(x)");
    }

    [Fact]
    public void Print_Derivative_ReturnsDdNotation()
    {
        var xSq = Expr.Pow(Expr.Variable("x"), Expr.Literal(2));
        var expr = Expr.Derivative(xSq, Expr.Variable("x"));
        ExpressionPrinter.Print(expr).Should().Be("d/dx (x ^ 2)");
    }

    [Fact]
    public void Print_IndefiniteIntegral_ReturnsIntegralNotation()
    {
        var expr = Expr.Integral(Expr.Variable("x"), Expr.Variable("x"));
        ExpressionPrinter.Print(expr).Should().Be("∫ x dx");
    }

    [Fact]
    public void Print_Vector_ReturnsBracketedList()
    {
        var expr = Expr.Vector(Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));
        ExpressionPrinter.Print(expr).Should().Be("[1, 2, 3]");
    }

    [Fact]
    public void Print_Matrix_ReturnsNestedBrackets()
    {
        var row1 = Expr.Vector(Expr.Literal(1), Expr.Literal(2));
        var row2 = Expr.Vector(Expr.Literal(3), Expr.Literal(4));
        var expr = Expr.Matrix(row1, row2);
        ExpressionPrinter.Print(expr).Should().Be("[[1, 2]; [3, 4]]");
    }

    [Fact]
    public void Print_Null_ReturnsNullString()
    {
        ExpressionPrinter.Print(Expr.Null).Should().Be("null");
    }

    [Fact]
    public void Print_Boolean_ReturnsTrueFalse()
    {
        ExpressionPrinter.Print(Expr.Boolean(true)).Should().Be("true");
        ExpressionPrinter.Print(Expr.Boolean(false)).Should().Be("false");
    }

    [Fact]
    public void Print_Equation_ReturnsEqualsNotation()
    {
        var expr = Expr.Equation(Expr.Variable("x"), Expr.Literal(5));
        ExpressionPrinter.Print(expr).Should().Be("x = 5");
    }

    [Fact]
    public void Print_Conditional_ReturnsIfThenElse()
    {
        var expr = Expr.Conditional(
            Expr.Boolean(true),
            Expr.Literal(1),
            Expr.Literal(0));
        ExpressionPrinter.Print(expr).Should().Be("if true then 1 else 0");
    }

    [Fact]
    public void Print_Factorial_ReturnsBangNotation()
    {
        var expr = Expr.Factorial(Expr.Literal(5));
        ExpressionPrinter.Print(expr).Should().Be("5!");
    }

    [Fact]
    public void Print_Limit_ReturnsLimNotation()
    {
        var expr = Expr.Limit(Expr.Variable("x"), Expr.Variable("x"), Expr.Literal(0));
        ExpressionPrinter.Print(expr).Should().Be("lim[x→0] x");
    }

    [Fact]
    public void Print_Summation_ReturnsSigmaNotation()
    {
        var expr = Expr.Summation(
            Expr.Variable("i"), Expr.Literal(1), Expr.Literal(10), Expr.Variable("i"));
        ExpressionPrinter.Print(expr).Should().Be("Σ[i=1..10] i");
    }

    [Fact]
    public void Print_Product_ReturnsPiNotation()
    {
        var expr = Expr.Product(
            Expr.Variable("i"), Expr.Literal(1), Expr.Literal(5), Expr.Variable("i"));
        ExpressionPrinter.Print(expr).Should().Be("Π[i=1..5] i");
    }

    [Fact]
    public void Print_Range_ReturnsDotDotNotation()
    {
        var expr = Expr.Range(Expr.Literal(1), Expr.Literal(10));
        ExpressionPrinter.Print(expr).Should().Be("1..10");
    }

    [Fact]
    public void Print_Interval_ReturnsBracketNotation()
    {
        var expr = Expr.Interval(Expr.Literal(1), Expr.Literal(10));
        ExpressionPrinter.Print(expr).Should().Be("[1, 10]");
    }

    [Fact]
    public void Print_Set_ReturnsCurlyBraces()
    {
        var expr = Expr.Set(Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));
        ExpressionPrinter.Print(expr).Should().Be("{1, 2, 3}");
    }

    [Fact]
    public void Print_Complex_ReturnsComplexNotation()
    {
        var expr = Expr.Complex(Expr.Literal(1), Expr.Literal(2));
        ExpressionPrinter.Print(expr).Should().Be("(1 + 2i)");
    }

    [Fact]
    public void Print_Identity_ReturnsIdNotation()
    {
        var expr = Expr.Identity("add");
        ExpressionPrinter.Print(expr).Should().Be("id(add)");
    }

    [Fact]
    public void Print_Polynomial_ReturnsPolyNotation()
    {
        var expr = Expr.Polynomial(Expr.Variable("x"), Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));
        ExpressionPrinter.Print(expr).Should().Be("poly(x, deg=2)");
    }

    [Fact]
    public void Print_Assignment_ReturnsAssignNotation()
    {
        var expr = Expr.Assign(Expr.Variable("x"), Expr.Literal(5));
        ExpressionPrinter.Print(expr).Should().Be("x := 5");
    }

    [Fact]
    public void Print_Composition_ReturnsCircleNotation()
    {
        var f = Expr.Variable("f");
        var g = Expr.Variable("g");
        var expr = Expr.Compose(f, g);
        ExpressionPrinter.Print(expr).Should().Be("(f ∘ g)");
    }

    [Fact]
    public void Print_Tuple_ReturnsParenthesizedList()
    {
        var expr = Expr.Tuple(Expr.Literal(1), Expr.Literal(2), Expr.Literal(3));
        ExpressionPrinter.Print(expr).Should().Be("(1, 2, 3)");
    }

    [Fact]
    public void Print_Piecewise_ReturnsPiecewiseSyntax()
    {
        var expr = Expr.Piecewise(
            [new PiecewiseCase(Expr.Literal(1), Expr.Boolean(true))],
            Expr.Literal(0));
        ExpressionPrinter.Print(expr).Should().Be("piecewise(1 if true; otherwise 0)");
    }

    [Fact]
    public void Print_Multiply_ReturnsParenthesizedExpression()
    {
        var expr = Expr.Multiply(Expr.Variable("a"), Expr.Variable("b"));
        ExpressionPrinter.Print(expr).Should().Be("(a * b)");
    }
}

public class ExpressionPrettyPrinterTests
{
    [Fact]
    public void PrettyPrint_ComplexExpression_ReturnsFormattedString()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Add(Expr.Sin(x), Expr.Pow(x, Expr.Literal(2)));

        var result = ExpressionPrettyPrinter.Print(expr);

        result.Should().Be("(sin(x) + (x ^ 2))");
    }

    [Fact]
    public void PrettyPrint_Equation_ReturnsFormattedString()
    {
        var expr = Expr.Equation(
            Expr.Add(Expr.Variable("a"), Expr.Variable("b")),
            Expr.Literal(0));

        var result = ExpressionPrettyPrinter.Print(expr);

        result.Should().Be("(a + b) = 0");
    }

    [Fact]
    public void PrettyPrint_NestedFunction_ReturnsFormattedString()
    {
        var expr = Expr.Sin(Expr.Cos(Expr.Variable("x")));

        var result = ExpressionPrettyPrinter.Print(expr);

        result.Should().Be("sin(cos(x))");
    }
}

public class ExpressionComparerTests
{
    [Fact]
    public void Compare_EqualLiterals_ReturnsTrue()
    {
        var a = Expr.Literal(5);
        var b = Expr.Literal(5);

        ExpressionComparer.Compare(a, b).Should().BeTrue();
    }

    [Fact]
    public void Compare_DifferentLiterals_ReturnsFalse()
    {
        var a = Expr.Literal(5);
        var b = Expr.Literal(10);

        ExpressionComparer.Compare(a, b).Should().BeFalse();
    }

    [Fact]
    public void Compare_EqualExpressions_ReturnsTrue()
    {
        var a = Expr.Add(Expr.Variable("x"), Expr.Literal(1));
        var b = Expr.Add(Expr.Variable("x"), Expr.Literal(1));

        ExpressionComparer.Compare(a, b).Should().BeTrue();
    }

    [Fact]
    public void Compare_DifferentStructure_ReturnsFalse()
    {
        var a = Expr.Add(Expr.Variable("x"), Expr.Literal(1));
        var b = Expr.Multiply(Expr.Variable("x"), Expr.Literal(1));

        ExpressionComparer.Compare(a, b).Should().BeFalse();
    }

    [Fact]
    public void CompareTrees_EqualTrees_ReturnsTrue()
    {
        var a = Expr.Pow(Expr.Variable("x"), Expr.Literal(2));
        var b = Expr.Pow(Expr.Variable("x"), Expr.Literal(2));

        ExpressionComparer.Compare(a, b).Should().BeTrue();
    }

    [Fact]
    public void CompareTrees_DifferentTrees_ReturnsFalse()
    {
        var a = Expr.Pow(Expr.Variable("x"), Expr.Literal(2));
        var b = Expr.Pow(Expr.Variable("y"), Expr.Literal(2));

        ExpressionComparer.Instance.CompareTrees(a, b).Should().BeFalse();
    }
}

public class ExpressionHasherTests
{
    [Fact]
    public void Hash_SameExpression_ProducesSameHash()
    {
        var a = Expr.Add(Expr.Variable("x"), Expr.Literal(1));
        var b = Expr.Add(Expr.Variable("x"), Expr.Literal(1));

        ExpressionHasher.Hash(a).Should().Be(ExpressionHasher.Hash(b));
    }

    [Fact]
    public void Hash_DifferentExpressions_ProduceDifferentHashes()
    {
        var a = Expr.Literal(1);
        var b = Expr.Literal(2);

        ExpressionHasher.Hash(a).Should().NotBe(ExpressionHasher.Hash(b));
    }

    [Fact]
    public void Hash_ComplexTree_DeterministicHash()
    {
        var expr = Expr.Sin(Expr.Add(Expr.Variable("x"), Expr.Literal(3)));

        var hash1 = ExpressionHasher.Hash(expr);
        var hash2 = ExpressionHasher.Hash(expr);

        hash1.Should().Be(hash2);
    }

    [Fact]
    public void Hash_DifferentStructures_DifferentHash()
    {
        var a = Expr.Add(Expr.Literal(1), Expr.Literal(2));
        var b = Expr.Multiply(Expr.Literal(1), Expr.Literal(2));

        ExpressionHasher.Hash(a).Should().NotBe(ExpressionHasher.Hash(b));
    }
}

public class ExpressionWalkerTests
{
    [Fact]
    public void Walker_LeafNode_DoesNotThrow()
    {
        var walker = new ExpressionWalker();
        var act = () => Expr.Literal(42).Accept(walker);
        act.Should().NotThrow();
    }

    [Fact]
    public void Walker_BinaryExpression_VisitsBothChildren()
    {
        var visited = new List<string>();
        var walker = new CountingWalker(visited);

        var expr = Expr.Add(Expr.Variable("x"), Expr.Literal(5));
        expr.Accept(walker);

        visited.Should().Contain("Variable");
        visited.Should().Contain("Literal");
    }

    [Fact]
    public void Walker_FunctionCall_VisitsAllArguments()
    {
        var visited = new List<string>();
        var walker = new CountingWalker(visited);

        var expr = Expr.Sin(Expr.Variable("x"));
        expr.Accept(walker);

        visited.Should().Contain("FunctionCall");
        visited.Should().Contain("Variable");
    }
}

public class ExpressionNodeCounterTests
{
    [Fact]
    public void Count_Literal_ReturnsOne()
    {
        var expr = Expr.Literal(5);
        ExpressionNodeCounter.Count(expr).Should().Be(1);
    }

    [Fact]
    public void Count_BinaryTree_ReturnsCorrectCount()
    {
        var x = Expr.Variable("x");
        var two = Expr.Literal(2);
        var pow = Expr.Pow(x, two);
        var three = Expr.Literal(3);
        var add = Expr.Add(pow, three);

        ExpressionNodeCounter.Count(add).Should().Be(5);
    }

    [Fact]
    public void Count_DeeplyNested_ReturnsCorrectCount()
    {
        var expr = Expr.Add(
            Expr.Multiply(Expr.Literal(1), Expr.Literal(2)),
            Expr.Divide(Expr.Literal(3), Expr.Literal(4)));

        ExpressionNodeCounter.Count(expr).Should().Be(7);
    }
}

public class ExpressionStatisticsVisitorTests
{
    [Fact]
    public void Collect_Literal_ReturnsOneNode()
    {
        var expr = Expr.Literal(42);

        var stats = ExpressionStatisticsVisitor.Collect(expr);

        stats.NodeCount.Should().Be(1);
        stats.MaxDepth.Should().Be(1);
    }

    [Fact]
    public void Collect_WithVariables_ReportsVariables()
    {
        var expr = Expr.Add(Expr.Variable("x"), Expr.Variable("y"));

        var stats = ExpressionStatisticsVisitor.Collect(expr);

        stats.Variables.Should().Contain("x");
        stats.Variables.Should().Contain("y");
        stats.NodeCount.Should().Be(3);
    }

    [Fact]
    public void Collect_WithFunctions_ReportsFunctionNames()
    {
        var expr = Expr.Sin(Expr.Variable("x"));

        var stats = ExpressionStatisticsVisitor.Collect(expr);

        stats.Functions.Should().Contain("sin");
        stats.Variables.Should().Contain("x");
    }

    [Fact]
    public void Collect_ComplexExpression_CorrectStatistics()
    {
        var x = Expr.Variable("x");
        var expr = Expr.Add(
            Expr.Sin(x),
            Expr.Pow(x, Expr.Literal(2)));

        var stats = ExpressionStatisticsVisitor.Collect(expr);

        stats.Variables.Should().Contain("x");
        stats.Functions.Should().Contain("sin");
        stats.KindCounts.Should().ContainKey(ExpressionKind.Binary);
        stats.KindCounts[ExpressionKind.Binary].Should().Be(2);
    }

    [Fact]
    public void Collect_PiecewiseExpression_CorrectNodeCount()
    {
        var expr = Expr.Piecewise(
            [new PiecewiseCase(Expr.Literal(1), Expr.Boolean(true))],
            Expr.Literal(0));

        var stats = ExpressionStatisticsVisitor.Collect(expr);

        stats.KindCounts.Should().ContainKey(ExpressionKind.Piecewise);
        stats.NodeCount.Should().BeGreaterThan(1);
    }
}

public class ExpressionReplacerTests
{
    [Fact]
    public void Replace_VariableInAddition_ReplacesCorrectly()
    {
        var target = Expr.Variable("x");
        var replacement = Expr.Literal(5);
        var expr = Expr.Add(target, Expr.Literal(3));

        var result = ExpressionReplacer.Replace(expr, target, replacement);

        result.Should().Be(Expr.Add(Expr.Literal(5), Expr.Literal(3)));
    }

    [Fact]
    public void Replace_NoMatchingSubexpression_ReturnsOriginal()
    {
        var target = Expr.Variable("z");
        var replacement = Expr.Literal(99);
        var expr = Expr.Add(Expr.Variable("x"), Expr.Literal(3));

        var result = ExpressionReplacer.Replace(expr, target, replacement);

        result.Should().Be(expr);
    }

    [Fact]
    public void Replace_MultipleOccurrences_ReplacesAll()
    {
        var target = Expr.Variable("x");
        var replacement = Expr.Literal(2);
        var expr = Expr.Add(Expr.Variable("x"), Expr.Multiply(Expr.Variable("x"), Expr.Literal(3)));

        var result = ExpressionReplacer.Replace(expr, target, replacement);

        result.Should().Be(Expr.Add(Expr.Literal(2), Expr.Multiply(Expr.Literal(2), Expr.Literal(3))));
    }
}

public class RewriteRuleTests
{
    [Fact]
    public void Create_RuleWithCondition_AppliesWhenMet()
    {
        var rule = RewriteRule.Create(
            "double-literal",
            expr => expr is LiteralExpression l && l.Value == 2.0,
            expr => Expr.Literal(4),
            priority: 1);

        rule.Name.Should().Be("double-literal");
        rule.Priority.Should().Be(1);

        var result = rule.TryRewrite(Expr.Literal(2));
        result.Should().Be(Expr.Literal(4));
    }

    [Fact]
    public void Create_RuleWithCondition_ReturnsNullWhenNotMet()
    {
        var rule = RewriteRule.Create(
            "double-literal",
            expr => expr is LiteralExpression l && l.Value == 2.0,
            expr => Expr.Literal(4));

        var result = rule.TryRewrite(Expr.Literal(3));
        result.Should().BeNull();
    }

    [Fact]
    public void Constructor_WithLambdaFunction_CreatesRule()
    {
        var rule = RewriteRule.Create(
            "negate-self",
            expr => expr is BinaryExpression b &&
                    b.Operator == MathOperator.Add &&
                    b.Left is LiteralExpression l && l.Value == 0,
            expr => ((BinaryExpression)expr).Right);

        var expr = Expr.Add(Expr.Literal(0), Expr.Variable("x"));
        var result = rule.TryRewrite(expr);

        result.Should().Be(Expr.Variable("x"));
    }
}

public class RuleSetTests
{
    [Fact]
    public void Add_SingleRule_RuleAppearsInCollection()
    {
        var ruleSet = new RuleSet();
        var rule = RewriteRule.Create("test", _ => true, e => e);

        ruleSet.Add(rule);

        ruleSet.Rules.Should().Contain(rule);
    }

    [Fact]
    public void Rules_OrderedByPriorityDescending()
    {
        var ruleSet = new RuleSet();
        var lowPriority = RewriteRule.Create("low", _ => true, e => e, priority: 1);
        var highPriority = RewriteRule.Create("high", _ => true, e => e, priority: 10);

        ruleSet.Add(lowPriority);
        ruleSet.Add(highPriority);

        ruleSet.Rules[0].Name.Should().Be("high");
        ruleSet.Rules[1].Name.Should().Be("low");
    }

    [Fact]
    public void AddRange_MultipleRules_AllAdded()
    {
        var ruleSet = new RuleSet();
        var rules = new[]
        {
            RewriteRule.Create("r1", _ => true, e => e),
            RewriteRule.Create("r2", _ => true, e => e),
        };

        ruleSet.AddRange(rules);

        ruleSet.Rules.Should().HaveCount(2);
    }

    [Fact]
    public void Constructor_WithRules_InitializesCollection()
    {
        var rules = new[]
        {
            RewriteRule.Create("first", _ => true, e => e, priority: 5),
            RewriteRule.Create("second", _ => true, e => e, priority: 10),
        };

        var ruleSet = new RuleSet(rules);

        ruleSet.Rules.Should().HaveCount(2);
        ruleSet.Rules[0].Name.Should().Be("second");
    }
}

public class RewriteEngineTests
{
    [Fact]
    public void ApplyOnce_SingleRule_AppliesRule()
    {
        var ruleSet = new RuleSet();
        ruleSet.Add(RewriteRule.Create(
            "zero-add",
            expr => expr is BinaryExpression b &&
                    b.Operator == MathOperator.Add &&
                    b.Left is LiteralExpression l && l.Value == 0,
            expr => ((BinaryExpression)expr).Right));

        var engine = new RewriteEngine(ruleSet);
        var expr = Expr.Add(Expr.Literal(0), Expr.Variable("x"));

        var result = engine.ApplyOnce(expr);

        result.Should().Be(Expr.Variable("x"));
    }

    [Fact]
    public void ApplyOnce_NoMatchingRule_ReturnsOriginal()
    {
        var ruleSet = new RuleSet();
        ruleSet.Add(RewriteRule.Create(
            "never-matches",
            _ => false,
            _ => Expr.Literal(999)));

        var engine = new RewriteEngine(ruleSet);
        var expr = Expr.Add(Expr.Literal(1), Expr.Literal(2));

        var result = engine.ApplyOnce(expr);

        result.Should().Be(expr);
    }

    [Fact]
    public void ApplyToFixpoint_ConvergesWhenNoMoreChanges()
    {
        var ruleSet = new RuleSet();
        ruleSet.Add(RewriteRule.Create(
            "double-zero",
            expr => expr is BinaryExpression b &&
                    b.Operator == MathOperator.Add &&
                    b.Left is LiteralExpression l && l.Value == 0,
            expr => ((BinaryExpression)expr).Right));

        var engine = new RewriteEngine(ruleSet);
        var expr = Expr.Add(Expr.Literal(0), Expr.Variable("x"));

        var result = engine.ApplyToFixpoint(expr);

        result.Should().Be(Expr.Variable("x"));
    }

    [Fact]
    public void ApplyToFixpoint_AlreadyMinimal_ReturnsSame()
    {
        var ruleSet = new RuleSet();
        ruleSet.Add(RewriteRule.Create(
            "double-zero",
            expr => expr is BinaryExpression b &&
                    b.Operator == MathOperator.Add &&
                    b.Left is LiteralExpression l && l.Value == 0,
            expr => ((BinaryExpression)expr).Right));

        var engine = new RewriteEngine(ruleSet);
        var expr = Expr.Variable("x");

        var result = engine.ApplyToFixpoint(expr);

        result.Should().Be(expr);
    }

    [Fact]
    public void ApplyPasses_LimitedPasses_RespectsLimit()
    {
        var passCount = 0;
        var ruleSet = new RuleSet();
        ruleSet.Add(RewriteRule.Create(
            "increment",
            _ => true,
            _ => { passCount++; return Expr.Literal(passCount); }));

        var engine = new RewriteEngine(ruleSet);
        var expr = Expr.Literal(0);

        engine.ApplyPasses(expr, 3);

        passCount.Should().BeLessThanOrEqualTo(3);
    }

    [Fact]
    public void ApplyOnce_MultipleRules_AllAppliedInPriorityOrder()
    {
        var ruleSet = new RuleSet();
        ruleSet.Add(RewriteRule.Create(
            "replace-1",
            expr => expr is LiteralExpression l && l.Value == 1,
            _ => Expr.Literal(10),
            priority: 1));
        ruleSet.Add(RewriteRule.Create(
            "replace-10",
            expr => expr is LiteralExpression l && l.Value == 10,
            _ => Expr.Literal(100),
            priority: 2));

        var engine = new RewriteEngine(ruleSet);
        var result = engine.ApplyOnce(Expr.Literal(1));

        result.Should().Be(Expr.Literal(10));
    }

    [Fact]
    public void ApplyToFixpoint_ChainedRewrites_Complete()
    {
        var ruleSet = new RuleSet();
        ruleSet.Add(RewriteRule.Create(
            "replace-1",
            expr => expr is LiteralExpression l && l.Value == 1,
            _ => Expr.Literal(10),
            priority: 1));
        ruleSet.Add(RewriteRule.Create(
            "replace-10",
            expr => expr is LiteralExpression l && l.Value == 10,
            _ => Expr.Literal(100),
            priority: 1));

        var engine = new RewriteEngine(ruleSet);
        var result = engine.ApplyToFixpoint(Expr.Literal(1));

        result.Should().Be(Expr.Literal(100));
    }
}

public class ExpressionRewriterTests
{
    [Fact]
    public void Rewrite_SinglePassTransform_AppliesOnce()
    {
        var rewriter = new LiteralIncrementRewriter();
        var expr = Expr.Literal(5);

        var result = rewriter.Rewrite(expr);

        result.Should().Be(Expr.Literal(6));
    }
}

#region Helpers

internal class CountingWalker : ExpressionWalker
{
    private readonly List<string> _visited;

    public CountingWalker(List<string> visited)
    {
        _visited = visited;
    }

    public override void Visit(VariableExpression expression)
    {
        _visited.Add("Variable");
    }

    public override void Visit(LiteralExpression expression)
    {
        _visited.Add("Literal");
    }

    public override void Visit(BinaryExpression expression)
    {
        _visited.Add("Binary");
        base.Visit(expression);
    }

    public override void Visit(FunctionCallExpression expression)
    {
        _visited.Add("FunctionCall");
        base.Visit(expression);
    }
}

internal class LiteralIncrementRewriter : ExpressionRewriter
{
    public override MathVerse.Math.Expressions.Expression Visit(LiteralExpression expression) =>
        new LiteralExpression(expression.Value + 1);
}

#endregion
