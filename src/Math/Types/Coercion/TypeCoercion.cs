namespace MathVerse.Math.Types.Coercion;

/// <summary>Manages type coercions: finding conversion paths and costs.</summary>
public sealed class TypeCoercion
{
    private readonly List<CoercionRule> _rules = new();

    /// <summary>Registered coercion rules.</summary>
    public IReadOnlyList<CoercionRule> Rules => _rules;

    /// <summary>Creates a TypeCoercion with default numeric rules.</summary>
    public TypeCoercion()
    {
        RegisterDefaultRules();
    }

    /// <summary>Registers a coercion rule.</summary>
    public void RegisterRule(CoercionRule rule)
    {
        _rules.Add(rule);
    }

    /// <summary>Finds the best implicit conversion from one type to another.</summary>
    public ImplicitConversion? FindImplicitConversion(MathType from, MathType to)
    {
        if (from.Equals(to))
            return new ImplicitConversion(from, to, ConversionCost.Zero);

        if (from is ScalarType fs && to is ScalarType ts)
        {
            var cost = FindScalarWideningCost(fs, ts);
            if (cost is not null)
                return new ImplicitConversion(from, to, cost);
        }

        if (from is VectorType fv && to is VectorType tv)
        {
            if (fv.Dimension == tv.Dimension || !fv.Dimension.HasValue || !tv.Dimension.HasValue)
            {
                var elemConv = FindImplicitConversion(fv.ElementType, tv.ElementType);
                if (elemConv is not null)
                    return new ImplicitConversion(from, to, elemConv.Cost);
            }
        }

        if (from is MatrixType fm && to is MatrixType tm)
        {
            if (fm.Rows == tm.Rows && fm.Columns == tm.Columns)
            {
                var elemConv = FindImplicitConversion(fm.ElementType, tm.ElementType);
                if (elemConv is not null)
                    return new ImplicitConversion(from, to, elemConv.Cost);
            }
        }

        if (from is TensorType ftt && to is TensorType ttt)
        {
            if (ftt.Rank == ttt.Rank)
            {
                bool shapeMatch = true;
                for (int i = 0; i < ftt.Rank; i++)
                {
                    if (ftt.Shape[i] != ttt.Shape[i])
                    {
                        shapeMatch = false;
                        break;
                    }
                }

                if (shapeMatch)
                {
                    var elemConv = FindImplicitConversion(ftt.ElementType, ttt.ElementType);
                    if (elemConv is not null)
                        return new ImplicitConversion(from, to, elemConv.Cost);
                }
            }
        }

        foreach (var rule in _rules)
        {
            if (rule.From.Equals(from) && rule.To.Equals(to) && rule.IsImplicit)
            {
                return new ImplicitConversion(from, to, rule.Cost);
            }
        }

        return null;
    }

    /// <summary>Finds the best explicit conversion from one type to another.</summary>
    public ExplicitConversion? FindExplicitConversion(MathType from, MathType to)
    {
        var implicitConv = FindImplicitConversion(from, to);
        if (implicitConv is not null)
            return new ExplicitConversion(from, to, false);

        if (from is ScalarType && to is ScalarType)
        {
            return new ExplicitConversion(from, to, true);
        }

        return null;
    }

    /// <summary>Computes the conversion cost between two types.</summary>
    public ConversionCost GetConversionCost(MathType from, MathType to)
    {
        if (from.Equals(to)) return ConversionCost.Zero;

        var implicitConv = FindImplicitConversion(from, to);
        if (implicitConv is not null) return implicitConv.Cost;

        return ConversionCost.Impossible;
    }

    /// <summary>Whether an implicit conversion exists.</summary>
    public bool CanImplicitlyConvert(MathType from, MathType to)
    {
        return FindImplicitConversion(from, to) is not null;
    }

    /// <summary>Whether an explicit conversion exists.</summary>
    public bool CanExplicitlyConvert(MathType from, MathType to)
    {
        return FindExplicitConversion(from, to) is not null;
    }

    private ConversionCost? FindScalarWideningCost(ScalarType from, ScalarType to)
    {
        if (from.Equals(to)) return ConversionCost.Zero;

        var fromIndex = ScalarIndex(from);
        var toIndex = ScalarIndex(to);

        if (fromIndex >= 0 && toIndex >= 0 && fromIndex < toIndex)
        {
            return new ConversionCost(toIndex - fromIndex);
        }

        return null;
    }

    private static int ScalarIndex(ScalarType type) => type switch
    {
        IntegerType or TypedInteger => 0,
        RationalType => 1,
        RealType => 2,
        ComplexType => 3,
        _ => -1,
    };

    private void RegisterDefaultRules()
    {
        _rules.Add(new CoercionRule(IntegerType.Instance, RationalType.Instance,
            CoercionKind.ImplicitWidening, new ConversionCost(1)));
        _rules.Add(new CoercionRule(RationalType.Instance, RealType.Instance,
            CoercionKind.ImplicitWidening, new ConversionCost(1)));
        _rules.Add(new CoercionRule(RealType.Instance, ComplexType.Instance,
            CoercionKind.ImplicitWidening, new ConversionCost(1)));
        _rules.Add(new CoercionRule(IntegerType.Instance, RealType.Instance,
            CoercionKind.ImplicitWidening, new ConversionCost(2)));
        _rules.Add(new CoercionRule(IntegerType.Instance, ComplexType.Instance,
            CoercionKind.ImplicitWidening, new ConversionCost(3)));

        _rules.Add(new CoercionRule(ComplexType.Instance, RealType.Instance,
            CoercionKind.ImplicitNarrowing, new ConversionCost(10)));
        _rules.Add(new CoercionRule(RealType.Instance, RationalType.Instance,
            CoercionKind.ImplicitNarrowing, new ConversionCost(10)));
        _rules.Add(new CoercionRule(RationalType.Instance, IntegerType.Instance,
            CoercionKind.ImplicitNarrowing, new ConversionCost(10)));

        _rules.Add(new CoercionRule(RealType.Instance, IntegerType.Instance,
            CoercionKind.Explicit, new ConversionCost(100)));
        _rules.Add(new CoercionRule(ComplexType.Instance, IntegerType.Instance,
            CoercionKind.Explicit, new ConversionCost(100)));
    }
}
