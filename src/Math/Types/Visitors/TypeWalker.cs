namespace MathVerse.Math.Types.Visitors;

/// <summary>Walks a type tree without modification, visiting each type.</summary>
public sealed class TypeWalker : TypeVisitor<bool>
{
    private readonly Action<MathType> _visit;

    /// <summary>Creates a type walker that calls the action for each visited type.</summary>
    public TypeWalker(Action<MathType> visit)
    {
        _visit = visit;
    }

    /// <summary>Walks a type, visiting it and all children.</summary>
    public void Walk(MathType type)
    {
        Visit(type);
    }

    private bool VisitChild(MathType? child)
    {
        if (child is not null)
        {
            _visit(child);
            Visit(child);
        }
        return true;
    }

    private bool VisitChildren(IReadOnlyList<MathType> children)
    {
        foreach (var child in children)
        {
            _visit(child);
            Visit(child);
        }
        return true;
    }

    /// <inheritdoc/>
    public override bool VisitUnknown(UnknownType type) { _visit(type); return true; }
    /// <inheritdoc/>
    public override bool VisitError(ErrorType type) { _visit(type); return true; }
    /// <inheritdoc/>
    public override bool VisitUnit(UnitType type) { _visit(type); return true; }
    /// <inheritdoc/>
    public override bool VisitBoolean(BooleanType type) { _visit(type); return true; }
    /// <inheritdoc/>
    public override bool VisitInteger(IntegerType type) { _visit(type); return true; }
    /// <inheritdoc/>
    public override bool VisitTypedInteger(TypedInteger type) { _visit(type); return true; }
    /// <inheritdoc/>
    public override bool VisitRational(RationalType type) { _visit(type); return true; }
    /// <inheritdoc/>
    public override bool VisitReal(RealType type) { _visit(type); return true; }
    /// <inheritdoc/>
    public override bool VisitComplex(ComplexType type) { _visit(type); return true; }
    /// <inheritdoc/>
    public override bool VisitString(StringType type) { _visit(type); return true; }

    /// <inheritdoc/>
    public override bool VisitFunction(FunctionType type)
    {
        _visit(type);
        VisitChildren(type.ParameterTypes);
        VisitChild(type.ReturnType);
        return true;
    }

    /// <inheritdoc/>
    public override bool VisitVector(VectorType type)
    {
        _visit(type);
        VisitChild(type.ElementType);
        return true;
    }

    /// <inheritdoc/>
    public override bool VisitMatrix(MatrixType type)
    {
        _visit(type);
        VisitChild(type.ElementType);
        return true;
    }

    /// <inheritdoc/>
    public override bool VisitTensor(TensorType type)
    {
        _visit(type);
        VisitChild(type.ElementType);
        return true;
    }

    /// <inheritdoc/>
    public override bool VisitPolynomial(PolynomialType type)
    {
        _visit(type);
        VisitChild(type.CoefficientType);
        return true;
    }

    /// <inheritdoc/>
    public override bool VisitEquation(EquationType type)
    {
        _visit(type);
        VisitChild(type.LeftType);
        VisitChild(type.RightType);
        return true;
    }

    /// <inheritdoc/>
    public override bool VisitSet(SetType type)
    {
        _visit(type);
        VisitChild(type.ElementType);
        return true;
    }

    /// <inheritdoc/>
    public override bool VisitTuple(TupleType type)
    {
        _visit(type);
        VisitChildren(type.ElementTypes);
        return true;
    }

    /// <inheritdoc/>
    public override bool VisitSequence(SequenceType type)
    {
        _visit(type);
        VisitChild(type.ElementType);
        return true;
    }

    /// <inheritdoc/>
    public override bool VisitDomain(DomainType type)
    {
        _visit(type);
        VisitChild(type.ElementType);
        return true;
    }

    /// <inheritdoc/>
    public override bool VisitTypeParameter(TypeParameter type) { _visit(type); return true; }
    /// <inheritdoc/>
    public override bool VisitGenericType(GenericType type) { _visit(type); return true; }

    /// <inheritdoc/>
    public override bool VisitGenericInstantiation(GenericInstantiation type)
    {
        _visit(type);
        VisitChildren(type.TypeArguments);
        return true;
    }

    /// <inheritdoc/>
    public override bool VisitTypeVariable(TypeVariable type) { _visit(type); return true; }
}

/// <summary>Collects all types in a type tree.</summary>
public sealed class TypeCollector : TypeVisitor<List<MathType>>
{
    /// <summary>The collected types.</summary>
    public List<MathType> Types { get; } = new();

    private bool Collect(MathType type)
    {
        Types.Add(type);
        return true;
    }

    private bool CollectChildren(IReadOnlyList<MathType> children)
    {
        foreach (var c in children)
        {
            Types.Add(c);
            Visit(c);
        }
        return true;
    }

    /// <inheritdoc/>
    public override List<MathType> VisitUnknown(UnknownType type) { Collect(type); return Types; }
    /// <inheritdoc/>
    public override List<MathType> VisitError(ErrorType type) { Collect(type); return Types; }
    /// <inheritdoc/>
    public override List<MathType> VisitUnit(UnitType type) { Collect(type); return Types; }
    /// <inheritdoc/>
    public override List<MathType> VisitBoolean(BooleanType type) { Collect(type); return Types; }
    /// <inheritdoc/>
    public override List<MathType> VisitInteger(IntegerType type) { Collect(type); return Types; }
    /// <inheritdoc/>
    public override List<MathType> VisitTypedInteger(TypedInteger type) { Collect(type); return Types; }
    /// <inheritdoc/>
    public override List<MathType> VisitRational(RationalType type) { Collect(type); return Types; }
    /// <inheritdoc/>
    public override List<MathType> VisitReal(RealType type) { Collect(type); return Types; }
    /// <inheritdoc/>
    public override List<MathType> VisitComplex(ComplexType type) { Collect(type); return Types; }
    /// <inheritdoc/>
    public override List<MathType> VisitString(StringType type) { Collect(type); return Types; }

    /// <inheritdoc/>
    public override List<MathType> VisitFunction(FunctionType type)
    {
        Collect(type);
        CollectChildren(type.ParameterTypes);
        Visit(type.ReturnType);
        return Types;
    }

    /// <inheritdoc/>
    public override List<MathType> VisitVector(VectorType type) { Collect(type); Visit(type.ElementType); return Types; }
    /// <inheritdoc/>
    public override List<MathType> VisitMatrix(MatrixType type) { Collect(type); Visit(type.ElementType); return Types; }
    /// <inheritdoc/>
    public override List<MathType> VisitTensor(TensorType type) { Collect(type); Visit(type.ElementType); return Types; }
    /// <inheritdoc/>
    public override List<MathType> VisitPolynomial(PolynomialType type) { Collect(type); Visit(type.CoefficientType); return Types; }

    /// <inheritdoc/>
    public override List<MathType> VisitEquation(EquationType type)
    {
        Collect(type);
        Visit(type.LeftType);
        Visit(type.RightType);
        return Types;
    }

    /// <inheritdoc/>
    public override List<MathType> VisitSet(SetType type) { Collect(type); Visit(type.ElementType); return Types; }
    /// <inheritdoc/>
    public override List<MathType> VisitTuple(TupleType type) { Collect(type); CollectChildren(type.ElementTypes); return Types; }
    /// <inheritdoc/>
    public override List<MathType> VisitSequence(SequenceType type) { Collect(type); Visit(type.ElementType); return Types; }
    /// <inheritdoc/>
    public override List<MathType> VisitDomain(DomainType type) { Collect(type); Visit(type.ElementType); return Types; }
    /// <inheritdoc/>
    public override List<MathType> VisitTypeParameter(TypeParameter type) { Collect(type); return Types; }
    /// <inheritdoc/>
    public override List<MathType> VisitGenericType(GenericType type) { Collect(type); return Types; }

    /// <inheritdoc/>
    public override List<MathType> VisitGenericInstantiation(GenericInstantiation type)
    {
        Collect(type);
        CollectChildren(type.TypeArguments);
        return Types;
    }

    /// <inheritdoc/>
    public override List<MathType> VisitTypeVariable(TypeVariable type) { Collect(type); return Types; }
}
