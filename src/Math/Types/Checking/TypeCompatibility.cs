namespace MathVerse.Math.Types.Checking;

/// <summary>Determines type compatibility for assignments, conversions, and operations.</summary>
public sealed class TypeCompatibility
{
    /// <summary>Determines if two types are identical.</summary>
    public bool AreIdentical(MathType left, MathType right)
    {
        return left.Equals(right);
    }

    /// <summary>Determines if left is assignable from right.</summary>
    public bool IsAssignableFrom(MathType left, MathType right)
    {
        if (left.Equals(right)) return true;
        if (left is ErrorType || right is ErrorType) return true;
        if (right is UnknownType) return true;

        if (left is ScalarType ls && right is ScalarType rs)
        {
            return IsScalarConvertible(rs, ls);
        }

        if (left is VectorType lv && right is VectorType rv)
        {
            return IsAssignableFrom(lv.ElementType, rv.ElementType);
        }

        if (left is MatrixType lm && right is MatrixType rm)
        {
            return IsAssignableFrom(lm.ElementType, rm.ElementType);
        }

        if (left is TensorType lt && right is TensorType rt)
        {
            return IsAssignableFrom(lt.ElementType, rt.ElementType);
        }

        return false;
    }

    /// <summary>Determines if two types are compatible for an operator.</summary>
    public bool IsCompatibleForOperator(MathType left, MathType right, string op)
    {
        if (left.Equals(right)) return true;

        if (op is "+" or "-" or "*" or "/" or "^" or "%")
        {
            if (left is ScalarType ls && right is ScalarType rs)
                return true;
            if (left is VectorType && right is ScalarType) return true;
            if (left is ScalarType && right is VectorType) return true;
            return false;
        }

        if (op is "=" or "==" or "!=" or "<" or ">" or "<=" or ">=")
        {
            return IsComparable(left, right);
        }

        if (op is "&&" or "||")
        {
            return left.Equals(BooleanType.Instance) && right.Equals(BooleanType.Instance);
        }

        return false;
    }

    /// <summary>Determines if two types are comparable.</summary>
    public bool IsComparable(MathType left, MathType right)
    {
        if (left.Equals(right)) return true;
        if (left is ScalarType ls && right is ScalarType rs)
        {
            return ls.IsOrdered || rs.IsOrdered;
        }
        return false;
    }

    /// <summary>Determines if a type can be implicitly converted to another.</summary>
    public bool CanImplicitlyConvert(MathType from, MathType to)
    {
        if (from.Equals(to)) return true;
        if (to is ScalarType ts && from is ScalarType fs)
            return IsScalarConvertible(fs, ts);
        return false;
    }

    private static bool IsScalarConvertible(ScalarType from, ScalarType to)
    {
        var current = from;
        while (current is not null)
        {
            if (current.Equals(to)) return true;
            current = current.Supertype;
        }
        return false;
    }
}
