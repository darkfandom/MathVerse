namespace MathVerse.Math.Types.Visitors;

/// <summary>Compares two mathematical types structurally.</summary>
public sealed class TypeComparer : TypeVisitor<int>
{
    private readonly MathType _other;

    /// <summary>Creates a type comparer targeting the other type.</summary>
    public TypeComparer(MathType other)
    {
        _other = other;
    }

    /// <summary>Compares two types. Returns 0 if equal, negative if less, positive if greater.</summary>
    public int Compare(MathType left, MathType right)
    {
        var lk = LeftKindOrder(left);
        var rk = LeftKindOrder(right);
        int cmp = lk.CompareTo(rk);
        if (cmp != 0) return cmp;

        return CompareInternal(left, right);
    }

    /// <summary>Compares this comparer's target type with another.</summary>
    public int CompareWith(MathType other)
    {
        return Compare(_other, other);
    }

    private int CompareInternal(MathType left, MathType right)
    {
        if (left is FunctionType ftL && right is FunctionType ftR)
        {
            int cmp = ftL.Arity.CompareTo(ftR.Arity);
            if (cmp != 0) return cmp;
            cmp = CompareInternal(ftL.ReturnType, ftR.ReturnType);
            if (cmp != 0) return cmp;
            for (int i = 0; i < ftL.Arity; i++)
            {
                cmp = CompareInternal(ftL.ParameterTypes[i], ftR.ParameterTypes[i]);
                if (cmp != 0) return cmp;
            }
            return 0;
        }

        if (left is VectorType vtL && right is VectorType vtR)
        {
            int cmp = CompareInternal(vtL.ElementType, vtR.ElementType);
            if (cmp != 0) return cmp;
            return NullableIntCompare(vtL.Dimension, vtR.Dimension);
        }

        if (left is MatrixType mtL && right is MatrixType mtR)
        {
            int cmp = CompareInternal(mtL.ElementType, mtR.ElementType);
            if (cmp != 0) return cmp;
            cmp = NullableIntCompare(mtL.Rows, mtR.Rows);
            if (cmp != 0) return cmp;
            return NullableIntCompare(mtL.Columns, mtR.Columns);
        }

        if (left is TensorType ttL && right is TensorType ttR)
        {
            int cmp = CompareInternal(ttL.ElementType, ttR.ElementType);
            if (cmp != 0) return cmp;
            cmp = ttL.Rank.CompareTo(ttR.Rank);
            if (cmp != 0) return cmp;
            for (int i = 0; i < ttL.Rank; i++)
            {
                cmp = NullableIntCompare(ttL.Shape[i], ttR.Shape[i]);
                if (cmp != 0) return cmp;
            }
            return 0;
        }

        if (left is TupleType tL && right is TupleType tR)
        {
            int cmp = tL.Arity.CompareTo(tR.Arity);
            if (cmp != 0) return cmp;
            for (int i = 0; i < tL.Arity; i++)
            {
                cmp = CompareInternal(tL.ElementTypes[i], tR.ElementTypes[i]);
                if (cmp != 0) return cmp;
            }
            return 0;
        }

        return string.Compare(left.Name, right.Name, System.StringComparison.Ordinal);
    }

    private static int LeftKindOrder(MathType type) => type.Kind switch
    {
        TypeKind.Error => 0,
        TypeKind.Unknown => 1,
        TypeKind.Unit => 2,
        TypeKind.Boolean => 10,
        TypeKind.Integer => 20,
        TypeKind.Rational => 21,
        TypeKind.Real => 22,
        TypeKind.Complex => 23,
        TypeKind.String => 30,
        TypeKind.Scalar => 40,
        TypeKind.Function => 50,
        TypeKind.Vector => 60,
        TypeKind.Matrix => 61,
        TypeKind.Tensor => 62,
        TypeKind.Polynomial => 70,
        TypeKind.Equation => 71,
        TypeKind.Set => 80,
        TypeKind.Tuple => 90,
        TypeKind.Sequence => 91,
        TypeKind.Domain => 100,
        TypeKind.Generic => 110,
        TypeKind.Record => 120,
        _ => 999,
    };

    private static int NullableIntCompare(int? a, int? b)
    {
        if (a.HasValue && b.HasValue) return a.Value.CompareTo(b.Value);
        if (a.HasValue) return -1;
        if (b.HasValue) return 1;
        return 0;
    }

    /// <inheritdoc/>
    public override int VisitUnknown(UnknownType type) => 0;
    /// <inheritdoc/>
    public override int VisitError(ErrorType type) => 0;
    /// <inheritdoc/>
    public override int VisitUnit(UnitType type) => 0;
    /// <inheritdoc/>
    public override int VisitBoolean(BooleanType type) => 0;
    /// <inheritdoc/>
    public override int VisitInteger(IntegerType type) => 0;
    /// <inheritdoc/>
    public override int VisitTypedInteger(TypedInteger type) => 0;
    /// <inheritdoc/>
    public override int VisitRational(RationalType type) => 0;
    /// <inheritdoc/>
    public override int VisitReal(RealType type) => 0;
    /// <inheritdoc/>
    public override int VisitComplex(ComplexType type) => 0;
    /// <inheritdoc/>
    public override int VisitString(StringType type) => 0;
    /// <inheritdoc/>
    public override int VisitFunction(FunctionType type) => 0;
    /// <inheritdoc/>
    public override int VisitVector(VectorType type) => 0;
    /// <inheritdoc/>
    public override int VisitMatrix(MatrixType type) => 0;
    /// <inheritdoc/>
    public override int VisitTensor(TensorType type) => 0;
    /// <inheritdoc/>
    public override int VisitPolynomial(PolynomialType type) => 0;
    /// <inheritdoc/>
    public override int VisitEquation(EquationType type) => 0;
    /// <inheritdoc/>
    public override int VisitSet(SetType type) => 0;
    /// <inheritdoc/>
    public override int VisitTuple(TupleType type) => 0;
    /// <inheritdoc/>
    public override int VisitSequence(SequenceType type) => 0;
    /// <inheritdoc/>
    public override int VisitDomain(DomainType type) => 0;
    /// <inheritdoc/>
    public override int VisitTypeParameter(TypeParameter type) => 0;
    /// <inheritdoc/>
    public override int VisitGenericType(GenericType type) => 0;
    /// <inheritdoc/>
    public override int VisitGenericInstantiation(GenericInstantiation type) => 0;
    /// <inheritdoc/>
    public override int VisitTypeVariable(TypeVariable type) => 0;
}

/// <summary>Computes deterministic hash codes for mathematical types.</summary>
public sealed class TypeHasher : TypeVisitor<int>
{
    /// <summary>Computes a hash code for a type.</summary>
    public int Hash(MathType type)
    {
        return Visit(type);
    }

    private int HashChildren(IReadOnlyList<MathType> types)
    {
        var hash = new HashCode();
        foreach (var t in types)
        {
            hash.Add(Visit(t));
        }
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public override int VisitUnknown(UnknownType type) => 0;
    /// <inheritdoc/>
    public override int VisitError(ErrorType type) => 1;
    /// <inheritdoc/>
    public override int VisitUnit(UnitType type) => 2;
    /// <inheritdoc/>
    public override int VisitBoolean(BooleanType type) => 10;
    /// <inheritdoc/>
    public override int VisitInteger(IntegerType type) => 20;
    /// <inheritdoc/>
    public override int VisitTypedInteger(TypedInteger type) => HashCode.Combine(21, type.Value);
    /// <inheritdoc/>
    public override int VisitRational(RationalType type) => 22;
    /// <inheritdoc/>
    public override int VisitReal(RealType type) => 23;
    /// <inheritdoc/>
    public override int VisitComplex(ComplexType type) => 24;
    /// <inheritdoc/>
    public override int VisitString(StringType type) => 30;

    /// <inheritdoc/>
    public override int VisitFunction(FunctionType type)
    {
        var hash = new HashCode();
        hash.Add(50);
        hash.Add(type.Arity);
        hash.Add(HashChildren(type.ParameterTypes));
        hash.Add(Visit(type.ReturnType));
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public override int VisitVector(VectorType type) =>
        HashCode.Combine(60, Visit(type.ElementType), type.Dimension);

    /// <inheritdoc/>
    public override int VisitMatrix(MatrixType type) =>
        HashCode.Combine(61, Visit(type.ElementType), type.Rows, type.Columns);

    /// <inheritdoc/>
    public override int VisitTensor(TensorType type)
    {
        var hash = new HashCode();
        hash.Add(62);
        hash.Add(Visit(type.ElementType));
        foreach (var d in type.Shape)
            hash.Add(d);
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public override int VisitPolynomial(PolynomialType type) =>
        HashCode.Combine(70, Visit(type.CoefficientType), type.VariableCount, type.MaxDegree);

    /// <inheritdoc/>
    public override int VisitEquation(EquationType type) =>
        HashCode.Combine(71, Visit(type.LeftType), Visit(type.RightType), type.Operator);

    /// <inheritdoc/>
    public override int VisitSet(SetType type) =>
        HashCode.Combine(80, Visit(type.ElementType), type.Cardinality);

    /// <inheritdoc/>
    public override int VisitTuple(TupleType type)
    {
        var hash = new HashCode();
        hash.Add(90);
        hash.Add(type.Arity);
        foreach (var t in type.ElementTypes)
            hash.Add(Visit(t));
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public override int VisitSequence(SequenceType type) =>
        HashCode.Combine(91, Visit(type.ElementType), type.Length);

    /// <inheritdoc/>
    public override int VisitDomain(DomainType type) =>
        HashCode.Combine(100, type.DomainName, Visit(type.ElementType));

    /// <inheritdoc/>
    public override int VisitTypeParameter(TypeParameter type) =>
        HashCode.Combine(110, type.Name_);

    /// <inheritdoc/>
    public override int VisitGenericType(GenericType type) =>
        HashCode.Combine(111, type.TypeName, type.Arity);

    /// <inheritdoc/>
    public override int VisitGenericInstantiation(GenericInstantiation type)
    {
        var hash = new HashCode();
        hash.Add(112);
        hash.Add(VisitGenericType(type.Definition));
        foreach (var arg in type.TypeArguments)
            hash.Add(Visit(arg));
        return hash.ToHashCode();
    }

    /// <inheritdoc/>
    public override int VisitTypeVariable(TypeVariable type) => HashCode.Combine(130, type.Id);
}
