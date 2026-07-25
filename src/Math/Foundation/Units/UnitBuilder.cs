using MathVerse.Math.Foundation.Dimensions;

namespace MathVerse.Math.Foundation.Units;

public sealed class UnitBuilder
{
    private string _symbol = string.Empty;
    private string _name = string.Empty;
    private Dimension _dimension = Dimension.None;
    private UnitCategory _category = UnitCategory.Dimensionless;
    private double _scaleFactor = 1.0;
    private readonly List<string> _aliases = new();

    public UnitBuilder WithSymbol(string symbol)
    {
        _symbol = symbol;
        return this;
    }

    public UnitBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public UnitBuilder WithDimension(Dimension dimension)
    {
        _dimension = dimension;
        return this;
    }

    public UnitBuilder WithCategory(UnitCategory category)
    {
        _category = category;
        return this;
    }

    public UnitBuilder WithScaleFactor(double scaleFactor)
    {
        _scaleFactor = scaleFactor;
        return this;
    }

    public UnitBuilder WithAlias(string alias)
    {
        _aliases.Add(alias);
        return this;
    }

    public Unit Build()
    {
        return new Unit
        {
            Symbol = _symbol,
            Name = _name,
            Dimension = _dimension,
            Category = _category,
            ScaleFactor = _scaleFactor,
            Aliases = _aliases.ToImmutableArray()
        };
    }
}
