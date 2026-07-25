namespace MathVerse.Math.Types.Inference;

/// <summary>Solves type constraints using unification.</summary>
public sealed class ConstraintSolver
{
    private readonly List<TypeDiagnostic> _diagnostics = new();

    /// <summary>Diagnostics produced during solving.</summary>
    public IReadOnlyList<TypeDiagnostic> Diagnostics => _diagnostics;

    /// <summary>Solves a set of constraints and produces a substitution.</summary>
    public TypeSubstitution Solve(IReadOnlyList<TypeConstraint> constraints)
    {
        var substitution = new TypeSubstitution();
        _diagnostics.Clear();

        foreach (var constraint in constraints)
        {
            substitution = SolveConstraint(constraint, substitution);
        }

        return substitution;
    }

    /// <summary>Solves a single constraint against a given substitution.</summary>
    public TypeSubstitution SolveConstraint(TypeConstraint constraint, TypeSubstitution current)
    {
        return constraint.Kind switch
        {
            TypeConstraintKind.Equality => SolveEquality(constraint.Left, constraint.Right!, current),
            TypeConstraintKind.Subtype => SolveSubtype(constraint.Left, constraint.Right!, current),
            TypeConstraintKind.Numeric => SolveNumeric(constraint.Left, current),
            _ => current,
        };
    }

    private TypeSubstitution SolveEquality(MathType left, MathType right, TypeSubstitution current)
    {
        var l = current.ApplyTo(left);
        var r = current.ApplyTo(right);

        if (l.Equals(r)) return current;

        if (l is TypeVariable tvL)
        {
            return current.Add(tvL.Id, r);
        }

        if (r is TypeVariable tvR)
        {
            return current.Add(tvR.Id, l);
        }

        if (l is FunctionType ftL && r is FunctionType ftR)
        {
            if (ftL.Arity != ftR.Arity)
            {
                _diagnostics.Add(new TypeDiagnostic(
                    TypeDiagnosticCode.IncompatibleTypes,
                    $"Function arity mismatch: {ftL.Arity} vs {ftR.Arity}"));
                return current;
            }

            for (int i = 0; i < ftL.Arity; i++)
            {
                current = SolveEquality(ftL.ParameterTypes[i], ftR.ParameterTypes[i], current);
            }
            current = SolveEquality(ftL.ReturnType, ftR.ReturnType, current);
            return current;
        }

        if (l is VectorType vtL && r is VectorType vtR)
        {
            current = SolveEquality(vtL.ElementType, vtR.ElementType, current);
            if (vtL.Dimension != vtR.Dimension)
            {
                _diagnostics.Add(new TypeDiagnostic(
                    TypeDiagnosticCode.InvalidTensorDimensions,
                    $"Vector dimension mismatch: {vtL.Dimension} vs {vtR.Dimension}"));
            }
            return current;
        }

        if (l is MatrixType mtL && r is MatrixType mtR)
        {
            current = SolveEquality(mtL.ElementType, mtR.ElementType, current);
            if (mtL.Rows != mtR.Rows || mtL.Columns != mtR.Columns)
            {
                _diagnostics.Add(new TypeDiagnostic(
                    TypeDiagnosticCode.InvalidTensorDimensions,
                    $"Matrix dimension mismatch: {mtL.Rows}×{mtL.Columns} vs {mtR.Rows}×{mtR.Columns}"));
            }
            return current;
        }

        if (l is TensorType ttL && r is TensorType ttR)
        {
            current = SolveEquality(ttL.ElementType, ttR.ElementType, current);
            if (ttL.Rank != ttR.Rank)
            {
                _diagnostics.Add(new TypeDiagnostic(
                    TypeDiagnosticCode.InvalidTensorDimensions,
                    $"Tensor rank mismatch: {ttL.Rank} vs {ttR.Rank}"));
            }
            return current;
        }

        if (l is TupleType tupleL && r is TupleType tupleR)
        {
            if (tupleL.Arity != tupleR.Arity)
            {
                _diagnostics.Add(new TypeDiagnostic(
                    TypeDiagnosticCode.IncompatibleTypes,
                    $"Tuple arity mismatch: {tupleL.Arity} vs {tupleR.Arity}"));
                return current;
            }

            for (int i = 0; i < tupleL.Arity; i++)
            {
                current = SolveEquality(tupleL.ElementTypes[i], tupleR.ElementTypes[i], current);
            }
            return current;
        }

        if (l is SetType stL && r is SetType stR)
        {
            current = SolveEquality(stL.ElementType, stR.ElementType, current);
            return current;
        }

        if (l is SequenceType seqL && r is SequenceType seqR)
        {
            current = SolveEquality(seqL.ElementType, seqR.ElementType, current);
            return current;
        }

        if (l is PolynomialType polyL && r is PolynomialType polyR)
        {
            current = SolveEquality(polyL.CoefficientType, polyR.CoefficientType, current);
            return current;
        }

        if (l is EquationType eqL && r is EquationType eqR)
        {
            current = SolveEquality(eqL.LeftType, eqR.LeftType, current);
            current = SolveEquality(eqL.RightType, eqR.RightType, current);
            return current;
        }

        _diagnostics.Add(new TypeDiagnostic(
            TypeDiagnosticCode.IncompatibleTypes,
            $"Cannot unify {l.Name} with {r.Name}"));
        return current;
    }

    private TypeSubstitution SolveSubtype(MathType child, MathType parent, TypeSubstitution current)
    {
        var c = current.ApplyTo(child);
        var p = current.ApplyTo(parent);

        if (c.Equals(p)) return current;

        if (c is TypeVariable tvC)
        {
            return current.Add(tvC.Id, p);
        }

        if (c is ScalarType cs && p is ScalarType ps)
        {
            if (IsSubtypeOfScalar(cs, ps)) return current;

            _diagnostics.Add(new TypeDiagnostic(
                TypeDiagnosticCode.IncompatibleTypes,
                $"Type {cs.Name} is not a subtype of {ps.Name}"));
            return current;
        }

        if (c is VectorType cv && p is VectorType pv)
        {
            current = SolveSubtype(cv.ElementType, pv.ElementType, current);
            return current;
        }

        if (c is MatrixType cm && p is MatrixType pm)
        {
            current = SolveSubtype(cm.ElementType, pm.ElementType, current);
            return current;
        }

        _diagnostics.Add(new TypeDiagnostic(
            TypeDiagnosticCode.IncompatibleTypes,
            $"Cannot establish subtype relation between {c.Name} and {p.Name}"));
        return current;
    }

    private TypeSubstitution SolveNumeric(MathType type, TypeSubstitution current)
    {
        var t = current.ApplyTo(type);

        if (t is TypeVariable tv)
        {
            return current;
        }

        if (t is ScalarType s && s.IsNumeric) return current;

        _diagnostics.Add(new TypeDiagnostic(
            TypeDiagnosticCode.IncompatibleTypes,
            $"Type {t.Name} is not numeric"));
        return current;
    }

    private static bool IsSubtypeOfScalar(ScalarType child, ScalarType parent)
    {
        var current = child;
        while (current is not null)
        {
            if (current.Equals(parent)) return true;
            current = current.Supertype;
        }
        return false;
    }
}

/// <summary>Diagnostic produced during constraint solving.</summary>
public sealed class TypeDiagnostic : IEquatable<TypeDiagnostic>
{
    /// <summary>The diagnostic code.</summary>
    public TypeDiagnosticCode Code { get; }

    /// <summary>The diagnostic message.</summary>
    public string Message { get; }

    /// <summary>Creates a type diagnostic.</summary>
    public TypeDiagnostic(TypeDiagnosticCode code, string message)
    {
        Code = code;
        Message = message;
    }

    /// <inheritdoc/>
    public bool Equals(TypeDiagnostic? other) =>
        other is not null && other.Code == Code && other.Message == Message;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as TypeDiagnostic);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Code, Message);

    /// <inheritdoc/>
    public override string ToString() => $"[{Code}] {Message}";
}

/// <summary>Type diagnostic codes.</summary>
public enum TypeDiagnosticCode
{
    /// <summary>Incompatible types.</summary>
    IncompatibleTypes = 6001,
    /// <summary>Ambiguous conversion.</summary>
    AmbiguousConversion = 6002,
    /// <summary>Impossible coercion.</summary>
    ImpossibleCoercion = 6003,
    /// <summary>Invalid generic argument.</summary>
    InvalidGenericArgument = 6004,
    /// <summary>Unresolved type.</summary>
    UnresolvedType = 6005,
    /// <summary>Recursive type.</summary>
    RecursiveType = 6006,
    /// <summary>Cyclic constraint.</summary>
    CyclicConstraint = 6007,
    /// <summary>Invalid tensor dimensions.</summary>
    InvalidTensorDimensions = 6008,
    /// <summary>Invalid matrix multiplication.</summary>
    InvalidMatrixMultiplication = 6009,
    /// <summary>Incompatible algebraic structures.</summary>
    IncompatibleAlgebraicStructures = 6010,
}
