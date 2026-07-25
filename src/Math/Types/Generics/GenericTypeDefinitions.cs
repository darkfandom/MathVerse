namespace MathVerse.Math.Types.Generics;

/// <summary>Provides pre-built generic type definitions for common mathematical types.</summary>
public static class GenericTypeDefinitions
{
    private static readonly TypeParameter T = new("T",
        new[] { new GenericConstraint(GenericConstraintKind.Numeric) });

    private static readonly TypeParameter TKey = new("TKey",
        new[] { new GenericConstraint(GenericConstraintKind.NotNull) });
    private static readonly TypeParameter TValue = new("TValue");

    /// <summary>Vector&lt;T&gt;</summary>
    public static readonly GenericType Vector =
        new("Vector", new[] { T });

    /// <summary>Matrix&lt;T&gt;</summary>
    public static readonly GenericType Matrix =
        new("Matrix", new[] { T });

    /// <summary>Tensor&lt;T&gt;</summary>
    public static readonly GenericType Tensor =
        new("Tensor", new[] { T });

    /// <summary>Set&lt;T&gt;</summary>
    public static readonly GenericType Set =
        new("Set", new[] { new TypeParameter("T") });

    /// <summary>Seq&lt;T&gt;</summary>
    public static readonly GenericType Sequence =
        new("Seq", new[] { new TypeParameter("T") });

    /// <summary>Tuple&lt;T1, T2&gt;</summary>
    public static readonly GenericType Tuple2 =
        new("Tuple", new[]
        {
            new TypeParameter("T1"),
            new TypeParameter("T2"),
        });

    /// <summary>Tuple&lt;T1, T2, T3&gt;</summary>
    public static readonly GenericType Tuple3 =
        new("Tuple", new[]
        {
            new TypeParameter("T1"),
            new TypeParameter("T2"),
            new TypeParameter("T3"),
        });

    /// <summary>Poly&lt;T&gt;</summary>
    public static readonly GenericType Polynomial =
        new("Poly", new[] { T });

    /// <summary>Dictionary&lt;TKey, TValue&gt;</summary>
    public static readonly GenericType Dictionary =
        new("Dictionary", new[] { TKey, TValue });
}
