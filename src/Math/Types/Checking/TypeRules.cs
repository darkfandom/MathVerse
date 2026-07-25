namespace MathVerse.Math.Types.Checking;

/// <summary>Defines static type rules for mathematical operations.</summary>
public static class TypeRules
{
    /// <summary>The numeric type promotion ladder.</summary>
    public static readonly ScalarType[] PromotionLadder = new ScalarType[]
    {
        IntegerType.Instance,
        RationalType.Instance,
        RealType.Instance,
        ComplexType.Instance,
    };

    /// <summary>Promotes two numeric types to their common supertype.</summary>
    public static MathType Promote(MathType left, MathType right)
    {
        if (left is ScalarType ls && right is ScalarType rs)
        {
            return PromoteScalar(ls, rs);
        }
        return UnknownType.Instance;
    }

    /// <summary>Promotes two scalar types.</summary>
    public static ScalarType PromoteScalar(ScalarType left, ScalarType right)
    {
        int li = PromotionLadder.IndexOf(left);
        int ri = PromotionLadder.IndexOf(right);

        if (li < 0 && ri < 0) return ComplexType.Instance;
        if (li < 0) return right;
        if (ri < 0) return left;

        return PromotionLadder[System.Math.Max(li, ri)];
    }

    /// <summary>Determines the result type of an arithmetic operation.</summary>
    public static MathType ArithmeticResult(MathType left, MathType right)
    {
        return Promote(left, right);
    }

    /// <summary>Determines the result type of an exponentiation.</summary>
    public static MathType ExponentiationResult(MathType baseType, MathType exponentType)
    {
        if (exponentType is TypedInteger ti && ti.Value >= 0)
        {
            return baseType;
        }

        if (baseType is ScalarType && exponentType is ScalarType)
        {
            if (exponentType is IntegerType || exponentType is TypedInteger)
                return baseType;
            return ComplexType.Instance;
        }

        return ComplexType.Instance;
    }

    /// <summary>Determines the result type of a comparison operation.</summary>
    public static MathType ComparisonResult(MathType left, MathType right)
    {
        return BooleanType.Instance;
    }

    /// <summary>Determines the result type of a logical operation.</summary>
    public static MathType LogicalResult(MathType left, MathType right)
    {
        return BooleanType.Instance;
    }

    /// <summary>Determines the result type of vector addition.</summary>
    public static MathType VectorAddResult(MathType left, MathType right)
    {
        if (left is VectorType lv && right is VectorType rv)
        {
            var elemType = Promote(lv.ElementType, rv.ElementType);
            return new VectorType(elemType, lv.Dimension);
        }
        return UnknownType.Instance;
    }

    /// <summary>Determines the result type of matrix multiplication.</summary>
    public static MathType MatrixMultiplyResult(MatrixType left, MatrixType right)
    {
        var elemType = Promote(left.ElementType, right.ElementType);
        int? rows = left.Rows;
        int? cols = right.Columns;
        return new MatrixType(elemType, rows, cols);
    }

    /// <summary>Determines the result type of dot product.</summary>
    public static MathType DotProductResult(VectorType left, VectorType right)
    {
        return Promote(left.ElementType, right.ElementType);
    }

    /// <summary>Determines the result type of cross product.</summary>
    public static MathType CrossProductResult(VectorType left, VectorType right)
    {
        return new VectorType(Promote(left.ElementType, right.ElementType), 3);
    }

    /// <summary>Determines the result type of function application.</summary>
    public static MathType ApplicationResult(MathType funcType, IReadOnlyList<MathType> argTypes)
    {
        if (funcType is FunctionType ft)
        {
            if (ft.Arity != argTypes.Count)
                return ErrorType.Instance;
            return ft.ReturnType;
        }
        return UnknownType.Instance;
    }

    /// <summary>Determines the result type of derivative.</summary>
    public static MathType DerivativeResult(MathType bodyType, MathType variableType)
    {
        return bodyType;
    }

    /// <summary>Determines the result type of integral.</summary>
    public static MathType IntegralResult(MathType bodyType, MathType variableType)
    {
        return bodyType;
    }

    /// <summary>Determines the result type of limit.</summary>
    public static MathType LimitResult(MathType bodyType)
    {
        return bodyType;
    }

    /// <summary>Determines the result type of summation.</summary>
    public static MathType SummationResult(MathType bodyType)
    {
        return bodyType;
    }

    /// <summary>Determines the result type of factorial.</summary>
    public static MathType FactorialResult(MathType operandType)
    {
        if (operandType is IntegerType or TypedInteger)
            return IntegerType.Instance;
        return RealType.Instance;
    }
}
