using MathVerse.Math.Foundation.Dimensions;

namespace MathVerse.Math.Foundation.Analysis;

public static class DimensionChecker
{
    public static bool Check(Dimension a, Dimension b)
    {
        return a.IsCompatibleWith(b);
    }

    public static bool AreDimensionsCompatible(Dimension a, Dimension b)
    {
        return a.IsCompatibleWith(b);
    }

    public static bool CheckAddition(Dimension left, Dimension right)
    {
        return left.IsCompatibleWith(right);
    }

    public static bool CheckEquality(Dimension left, Dimension right)
    {
        return left.IsCompatibleWith(right);
    }

    public static bool CheckAssignment(Dimension target, Dimension source)
    {
        return target.IsCompatibleWith(source);
    }
}
