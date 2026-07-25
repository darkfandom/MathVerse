using MathVerse.Math.Foundation.Dimensions;
using MathVerse.Math.Types;

namespace MathVerse.Math.Foundation.Integration;

public static class DimensionalTypeBridge
{
    public static Dimension ToDimension(MathType type)
    {
        if (type is null) throw new ArgumentNullException(nameof(type));

        return type switch
        {
            IntegerType => Dimension.None,
            RationalType => Dimension.None,
            RealType => Dimension.None,
            ComplexType => Dimension.None,
            ScalarType => Dimension.None,
            VectorType => new DimensionBuilder().Length().Build(),
            MatrixType => Dimension.None,
            TensorType => Dimension.None,
            BooleanType => Dimension.None,
            StringType => Dimension.None,
            SetType => Dimension.None,
            FunctionType => Dimension.None,
            _ => Dimension.None
        };
    }

    public static MathType ToMathType(Dimension dim)
    {
        if (dim is null) throw new ArgumentNullException(nameof(dim));

        if (dim.IsDimensionless) return RealType.Instance;

        if (dim.Exponents.Count == 1)
        {
            var kvp = dim.Exponents.First();
            if (kvp.Key == BaseDimension.Length.ToString() && System.Math.Abs(kvp.Value - 1.0) < 1e-10)
                return new VectorType(RealType.Instance);
        }

        return RealType.Instance;
    }

    public static bool AreEquivalent(MathType type, Dimension dim)
    {
        if (type is null) throw new ArgumentNullException(nameof(type));
        if (dim is null) throw new ArgumentNullException(nameof(dim));

        var mappedDim = ToDimension(type);
        return mappedDim.IsCompatibleWith(dim);
    }
}
