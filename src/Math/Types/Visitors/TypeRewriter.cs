namespace MathVerse.Math.Types.Visitors;

/// <summary>Rewrites a type tree by replacing types according to a substitution.</summary>
public sealed class TypeRewriter : TypeVisitor<MathType>
{
    private readonly Func<MathType, MathType?> _rewrite;

    /// <summary>Creates a type rewriter with a custom rewrite function.</summary>
    public TypeRewriter(Func<MathType, MathType?> rewrite)
    {
        _rewrite = rewrite;
    }

    /// <summary>Rewrites a type, applying the rewrite function and recursing into children.</summary>
    public MathType Rewrite(MathType type)
    {
        var rewritten = _rewrite(type);
        if (rewritten is not null) return rewritten;
        return Visit(type);
    }

    /// <summary>Creates a substitution-based rewriter.</summary>
    public static TypeRewriter FromSubstitution(TypeSubstitution substitution)
    {
        return new TypeRewriter(type =>
        {
            if (type is TypeVariable tv)
                return substitution.Get(tv.Id);
            return null;
        });
    }

    /// <inheritdoc/>
    public override MathType VisitUnknown(UnknownType type) => type;
    /// <inheritdoc/>
    public override MathType VisitError(ErrorType type) => type;
    /// <inheritdoc/>
    public override MathType VisitUnit(UnitType type) => type;
    /// <inheritdoc/>
    public override MathType VisitBoolean(BooleanType type) => type;
    /// <inheritdoc/>
    public override MathType VisitInteger(IntegerType type) => type;
    /// <inheritdoc/>
    public override MathType VisitTypedInteger(TypedInteger type) => type;
    /// <inheritdoc/>
    public override MathType VisitRational(RationalType type) => type;
    /// <inheritdoc/>
    public override MathType VisitReal(RealType type) => type;
    /// <inheritdoc/>
    public override MathType VisitComplex(ComplexType type) => type;
    /// <inheritdoc/>
    public override MathType VisitString(StringType type) => type;

    /// <inheritdoc/>
    public override MathType VisitFunction(FunctionType type)
    {
        var newParams = type.ParameterTypes.Select(Rewrite).ToList();
        var newReturn = Rewrite(type.ReturnType);
        return new FunctionType(newParams, newReturn);
    }

    /// <inheritdoc/>
    public override MathType VisitVector(VectorType type)
    {
        return new VectorType(Rewrite(type.ElementType), type.Dimension);
    }

    /// <inheritdoc/>
    public override MathType VisitMatrix(MatrixType type)
    {
        return new MatrixType(Rewrite(type.ElementType), type.Rows, type.Columns);
    }

    /// <inheritdoc/>
    public override MathType VisitTensor(TensorType type)
    {
        return new TensorType(Rewrite(type.ElementType), type.Shape);
    }

    /// <inheritdoc/>
    public override MathType VisitPolynomial(PolynomialType type)
    {
        return new PolynomialType(Rewrite(type.CoefficientType), type.VariableCount, type.MaxDegree);
    }

    /// <inheritdoc/>
    public override MathType VisitEquation(EquationType type)
    {
        return new EquationType(Rewrite(type.LeftType), Rewrite(type.RightType), type.Operator);
    }

    /// <inheritdoc/>
    public override MathType VisitSet(SetType type)
    {
        return new SetType(Rewrite(type.ElementType), type.Cardinality);
    }

    /// <inheritdoc/>
    public override MathType VisitTuple(TupleType type)
    {
        return new TupleType(type.ElementTypes.Select(Rewrite).ToList());
    }

    /// <inheritdoc/>
    public override MathType VisitSequence(SequenceType type)
    {
        return new SequenceType(Rewrite(type.ElementType), type.Length);
    }

    /// <inheritdoc/>
    public override MathType VisitDomain(DomainType type)
    {
        return new DomainType(type.DomainName, Rewrite(type.ElementType));
    }

    /// <inheritdoc/>
    public override MathType VisitTypeParameter(TypeParameter type) => type;
    /// <inheritdoc/>
    public override MathType VisitGenericType(GenericType type) => type;

    /// <inheritdoc/>
    public override MathType VisitGenericInstantiation(GenericInstantiation type)
    {
        return new GenericInstantiation(type.Definition,
            type.TypeArguments.Select(Rewrite).ToList());
    }

    /// <inheritdoc/>
    public override MathType VisitTypeVariable(TypeVariable type) => type;
}
