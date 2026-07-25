namespace MathVerse.Math.CAS.Rewriting;

using MathVerse.Math.Expressions;
using MathVerse.Math.CAS.PatternMatching;
using System.Collections.Immutable;

public sealed class ExpressionEqualityComparer : IEqualityComparer<Expression>
{
    public static readonly ExpressionEqualityComparer Instance = new();

    public bool Equals(Expression? x, Expression? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.Equals(y);
    }

    public int GetHashCode(Expression obj) => obj.GetHashCode();
}