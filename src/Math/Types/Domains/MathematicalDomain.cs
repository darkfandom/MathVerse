namespace MathVerse.Math.Types.Domains;

/// <summary>Represents a mathematical domain (number system or algebraic structure).</summary>
public sealed class MathematicalDomain : IEquatable<MathematicalDomain>
{
    /// <summary>The symbol of the domain (e.g., "ℕ", "ℤ", "ℚ", "ℝ", "ℂ").</summary>
    public string Symbol { get; }

    /// <summary>The name of the domain.</summary>
    public string Name { get; }

    /// <summary>The element type for this domain.</summary>
    public MathType ElementType { get; }

    /// <summary>The algebraic structures this domain supports.</summary>
    public IReadOnlyList<AlgebraicStructure> Structures { get; }

    /// <summary>Whether this domain is commutative.</summary>
    public bool IsCommutative { get; }

    /// <summary>Whether this domain is ordered.</summary>
    public bool IsOrdered { get; }

    /// <summary>Whether this domain is algebraically closed.</summary>
    public bool IsAlgebraicallyClosed { get; }

    /// <summary>Whether this domain is a field.</summary>
    public bool IsField { get; }

    /// <summary>Whether this domain is finite.</summary>
    public bool IsFinite { get; }

    /// <summary>Cardinality. Null for infinite domains.</summary>
    public long? Cardinality { get; }

    /// <summary>The parent domain (superset).</summary>
    public MathematicalDomain? Parent { get; }

    /// <summary>Creates a mathematical domain.</summary>
    public MathematicalDomain(string symbol, string name, MathType elementType,
        IReadOnlyList<AlgebraicStructure>? structures = null,
        bool isCommutative = true, bool isOrdered = false,
        bool isAlgebraicallyClosed = false, bool isField = false,
        bool isFinite = false, long? cardinality = null,
        MathematicalDomain? parent = null)
    {
        Symbol = symbol;
        Name = name;
        ElementType = elementType;
        Structures = structures ?? Array.Empty<AlgebraicStructure>();
        IsCommutative = isCommutative;
        IsOrdered = isOrdered;
        IsAlgebraicallyClosed = isAlgebraicallyClosed;
        IsField = isField;
        IsFinite = isFinite;
        Cardinality = cardinality;
        Parent = parent;
    }

    /// <inheritdoc/>
    public bool Equals(MathematicalDomain? other) =>
        other is not null && other.Symbol == Symbol && other.Name == Name;

    /// <inheritdoc/>
    public override bool Equals(object? obj) => Equals(obj as MathematicalDomain);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Symbol, Name);

    /// <inheritdoc/>
    public override string ToString() => $"{Symbol} ({Name})";
}
