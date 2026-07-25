namespace MathVerse.TypeSystem.Tests;

public class ConstraintSolverTests
{
    [Fact]
    public void Solve_Equality_SameType()
    {
        var solver = new ConstraintSolver();
        var constraints = new[]
        {
            new TypeConstraint(TypeConstraintKind.Equality, RealType.Instance, RealType.Instance)
        };
        var sub = solver.Solve(constraints);
        sub.Should().NotBeNull();
    }

    [Fact]
    public void Solve_Equality_VariableWithType()
    {
        var solver = new ConstraintSolver();
        var tv = new TypeVariable(0);
        var constraints = new[]
        {
            new TypeConstraint(TypeConstraintKind.Equality, tv, RealType.Instance)
        };
        var sub = solver.Solve(constraints);
        sub.Contains(0).Should().BeTrue();
        sub.Get(0).Should().Be(RealType.Instance);
    }

    [Fact]
    public void Solve_Equality_TypeWithVariable()
    {
        var solver = new ConstraintSolver();
        var tv = new TypeVariable(0);
        var constraints = new[]
        {
            new TypeConstraint(TypeConstraintKind.Equality, IntegerType.Instance, tv)
        };
        var sub = solver.Solve(constraints);
        sub.Contains(0).Should().BeTrue();
        sub.Get(0).Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void Solve_TransitiveEquality()
    {
        var solver = new ConstraintSolver();
        var tv1 = new TypeVariable(0);
        var tv2 = new TypeVariable(1);
        var constraints = new[]
        {
            new TypeConstraint(TypeConstraintKind.Equality, tv1, tv2),
            new TypeConstraint(TypeConstraintKind.Equality, tv2, RealType.Instance),
        };
        var sub = solver.Solve(constraints);
        var resolved1 = sub.ApplyTo(tv1);
        resolved1.Should().Be(RealType.Instance);
    }

    [Fact]
    public void Solve_Subtype_Variable()
    {
        var solver = new ConstraintSolver();
        var tv = new TypeVariable(0);
        var constraints = new[]
        {
            new TypeConstraint(TypeConstraintKind.Subtype, tv, RealType.Instance)
        };
        var sub = solver.Solve(constraints);
        sub.Contains(0).Should().BeTrue();
    }

    [Fact]
    public void Solve_Numeric_Variable()
    {
        var solver = new ConstraintSolver();
        var tv = new TypeVariable(0);
        var constraints = new[]
        {
            new TypeConstraint(TypeConstraintKind.Numeric, tv)
        };
        var sub = solver.Solve(constraints);
        sub.Should().NotBeNull();
    }

    [Fact]
    public void Solve_Numeric_ConcreteType()
    {
        var solver = new ConstraintSolver();
        var constraints = new[]
        {
            new TypeConstraint(TypeConstraintKind.Numeric, RealType.Instance)
        };
        var sub = solver.Solve(constraints);
        solver.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Solve_NonNumeric_Type()
    {
        var solver = new ConstraintSolver();
        var constraints = new[]
        {
            new TypeConstraint(TypeConstraintKind.Numeric, BooleanType.Instance)
        };
        var sub = solver.Solve(constraints);
        solver.Diagnostics.Should().NotBeEmpty();
    }

    [Fact]
    public void Solve_FunctionEquality()
    {
        var solver = new ConstraintSolver();
        var tv = new TypeVariable(0);
        var ft1 = new FunctionType(new[] { RealType.Instance }, tv);
        var ft2 = new FunctionType(new[] { RealType.Instance }, IntegerType.Instance);
        var constraints = new[]
        {
            new TypeConstraint(TypeConstraintKind.Equality, ft1, ft2)
        };
        var sub = solver.Solve(constraints);
        sub.Get(0).Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void Solve_VectorEquality()
    {
        var solver = new ConstraintSolver();
        var tv = new TypeVariable(0);
        var vt1 = new VectorType(tv, 3);
        var vt2 = new VectorType(RealType.Instance, 3);
        var constraints = new[]
        {
            new TypeConstraint(TypeConstraintKind.Equality, vt1, vt2)
        };
        var sub = solver.Solve(constraints);
        sub.Get(0).Should().Be(RealType.Instance);
    }

    [Fact]
    public void Solve_MatrixEquality()
    {
        var solver = new ConstraintSolver();
        var tv = new TypeVariable(0);
        var mt1 = new MatrixType(tv, 2, 2);
        var mt2 = new MatrixType(RealType.Instance, 2, 2);
        var constraints = new[]
        {
            new TypeConstraint(TypeConstraintKind.Equality, mt1, mt2)
        };
        var sub = solver.Solve(constraints);
        sub.Get(0).Should().Be(RealType.Instance);
    }

    [Fact]
    public void Solve_TupleEquality()
    {
        var solver = new ConstraintSolver();
        var tv = new TypeVariable(0);
        var tt1 = new TupleType(new MathType[] { tv, IntegerType.Instance });
        var tt2 = new TupleType(new MathType[] { RealType.Instance, IntegerType.Instance });
        var constraints = new[]
        {
            new TypeConstraint(TypeConstraintKind.Equality, tt1, tt2)
        };
        var sub = solver.Solve(constraints);
        sub.Get(0).Should().Be(RealType.Instance);
    }

    [Fact]
    public void Solve_SetEquality()
    {
        var solver = new ConstraintSolver();
        var tv = new TypeVariable(0);
        var st1 = new SetType(tv);
        var st2 = new SetType(RealType.Instance);
        var constraints = new[]
        {
            new TypeConstraint(TypeConstraintKind.Equality, st1, st2)
        };
        var sub = solver.Solve(constraints);
        sub.Get(0).Should().Be(RealType.Instance);
    }

    [Fact]
    public void Solve_SequenceEquality()
    {
        var solver = new ConstraintSolver();
        var tv = new TypeVariable(0);
        var seq1 = new SequenceType(tv);
        var seq2 = new SequenceType(IntegerType.Instance);
        var constraints = new[]
        {
            new TypeConstraint(TypeConstraintKind.Equality, seq1, seq2)
        };
        var sub = solver.Solve(constraints);
        sub.Get(0).Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void Solve_PolynomialEquality()
    {
        var solver = new ConstraintSolver();
        var tv = new TypeVariable(0);
        var pt1 = new PolynomialType(tv);
        var pt2 = new PolynomialType(RealType.Instance);
        var constraints = new[]
        {
            new TypeConstraint(TypeConstraintKind.Equality, pt1, pt2)
        };
        var sub = solver.Solve(constraints);
        sub.Get(0).Should().Be(RealType.Instance);
    }

    [Fact]
    public void Solve_EquationEquality()
    {
        var solver = new ConstraintSolver();
        var tv = new TypeVariable(0);
        var eq1 = new EquationType(tv, IntegerType.Instance);
        var eq2 = new EquationType(RealType.Instance, IntegerType.Instance);
        var constraints = new[]
        {
            new TypeConstraint(TypeConstraintKind.Equality, eq1, eq2)
        };
        var sub = solver.Solve(constraints);
        sub.Get(0).Should().Be(RealType.Instance);
    }

    [Fact]
    public void Solve_Unification_MultipleVariables()
    {
        var solver = new ConstraintSolver();
        var tv0 = new TypeVariable(0);
        var tv1 = new TypeVariable(1);
        var constraints = new[]
        {
            new TypeConstraint(TypeConstraintKind.Equality, tv0, RealType.Instance),
            new TypeConstraint(TypeConstraintKind.Equality, tv1, IntegerType.Instance),
        };
        var sub = solver.Solve(constraints);
        sub.ApplyTo(tv0).Should().Be(RealType.Instance);
        sub.ApplyTo(tv1).Should().Be(IntegerType.Instance);
    }

    [Fact]
    public void Solve_IncompatibleTypes_ProducesDiagnostic()
    {
        var solver = new ConstraintSolver();
        var constraints = new[]
        {
            new TypeConstraint(TypeConstraintKind.Equality, RealType.Instance, BooleanType.Instance)
        };
        solver.Solve(constraints);
        solver.Diagnostics.Should().NotBeEmpty();
    }

    [Fact]
    public void Solve_EmptyConstraints()
    {
        var solver = new ConstraintSolver();
        var sub = solver.Solve(Array.Empty<TypeConstraint>());
        sub.Should().NotBeNull();
        sub.Count.Should().Be(0);
    }

    [Fact]
    public void TypeConstraint_ToString_Equality()
    {
        var c = new TypeConstraint(TypeConstraintKind.Equality, RealType.Instance, IntegerType.Instance);
        c.ToString().Should().Be("Real = Integer");
    }

    [Fact]
    public void TypeConstraint_ToString_Numeric()
    {
        var c = new TypeConstraint(TypeConstraintKind.Numeric, RealType.Instance);
        c.ToString().Should().Be("numeric(Real)");
    }

    [Fact]
    public void TypeConstraint_Equals_Null()
    {
        var c = new TypeConstraint(TypeConstraintKind.Equality, RealType.Instance, IntegerType.Instance);
        c.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void TypeConstraint_Equals_Object()
    {
        var c1 = new TypeConstraint(TypeConstraintKind.Equality, RealType.Instance, IntegerType.Instance);
        object c2 = new TypeConstraint(TypeConstraintKind.Equality, RealType.Instance, IntegerType.Instance);
        c1.Equals(c2).Should().BeTrue();
    }

    [Fact]
    public void TypeDiagnostic_Code()
    {
        var d = new TypeDiagnostic(TypeDiagnosticCode.IncompatibleTypes, "test");
        d.Code.Should().Be(TypeDiagnosticCode.IncompatibleTypes);
    }

    [Fact]
    public void TypeDiagnostic_Message()
    {
        var d = new TypeDiagnostic(TypeDiagnosticCode.IncompatibleTypes, "msg");
        d.Message.Should().Be("msg");
    }

    [Fact]
    public void TypeDiagnostic_ToString()
    {
        var d = new TypeDiagnostic(TypeDiagnosticCode.IncompatibleTypes, "msg");
        d.ToString().Should().Be("[IncompatibleTypes] msg");
    }

    [Fact]
    public void TypeDiagnostic_Equals()
    {
        var d1 = new TypeDiagnostic(TypeDiagnosticCode.IncompatibleTypes, "msg");
        var d2 = new TypeDiagnostic(TypeDiagnosticCode.IncompatibleTypes, "msg");
        d1.Equals(d2).Should().BeTrue();
    }

    [Fact]
    public void TypeDiagnostic_NotEquals()
    {
        var d1 = new TypeDiagnostic(TypeDiagnosticCode.IncompatibleTypes, "msg1");
        var d2 = new TypeDiagnostic(TypeDiagnosticCode.IncompatibleTypes, "msg2");
        d1.Equals(d2).Should().BeFalse();
    }

    [Fact]
    public void TypeDiagnostic_GetHashCode()
    {
        var d = new TypeDiagnostic(TypeDiagnosticCode.IncompatibleTypes, "msg");
        d.GetHashCode().Should().Be(d.GetHashCode());
    }
}
