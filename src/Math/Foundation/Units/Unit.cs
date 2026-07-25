namespace MathVerse.Math.Foundation.Units;

using MathVerse.Math.Foundation.Dimensions;

public sealed record Unit
{
    public string Symbol { get; init; } = "";

    public string Name { get; init; } = "";

    public Dimension Dimension { get; init; } = Dimension.None;

    public UnitCategory Category { get; init; } = UnitCategory.Other;

    public double ScaleFactor { get; init; } = 1.0;

    public ImmutableArray<string> Aliases { get; init; } = ImmutableArray<string>.Empty;

    public bool IsBaseUnit => Dimension.IsBaseDimension;

    public bool IsDerivedUnit => !Dimension.IsDimensionless && !Dimension.IsBaseDimension;

    public bool Equals(Unit? other)
    {
        if (other is null) return false;
        return Symbol == other.Symbol &&
               Name == other.Name &&
               Dimension.Equals(other.Dimension) &&
               Category == other.Category &&
               ScaleFactor == other.ScaleFactor &&
               Aliases.SequenceEqual(other.Aliases);
    }

    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Symbol);
        hash.Add(Name);
        hash.Add(Dimension);
        hash.Add(Category);
        hash.Add(ScaleFactor);
        foreach (var alias in Aliases)
            hash.Add(alias);
        return hash.ToHashCode();
    }

    public Unit WithPrefix(UnitPrefix prefix) => this with
    {
        Symbol = $"{prefix.Symbol}{Symbol}",
        Name = $"{prefix.Name}{Name}",
        ScaleFactor = ScaleFactor * prefix.Factor
    };

    public override string ToString() => Symbol;
}
