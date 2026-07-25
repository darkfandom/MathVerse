namespace MathVerse.Math.Types.Algebra;

/// <summary>Determines the algebraic structure of mathematical types.</summary>
public static class AlgebraicClassifier
{
    /// <summary>Classifies the algebraic structure of a type.</summary>
    public static IReadOnlyList<AlgebraicStructure> Classify(MathType type)
    {
        var structures = new List<AlgebraicStructure>();

        if (type is ScalarType scalar)
        {
            var chain = ClassifyScalar(scalar);
            structures.AddRange(chain);
        }
        else if (type is VectorType vector)
        {
            var elemStructures = Classify(vector.ElementType);
            if (elemStructures.OfType<Field>().FirstOrDefault() is Field f)
            {
                var vs = new VectorSpace(vector, f);
                structures.Add(vs);
                structures.Add(new InnerProductSpace(vector, vs));
                structures.Add(new MetricSpace(vector, true));
            }
        }
        else if (type is MatrixType matrix)
        {
            var elemStructures = Classify(matrix.ElementType);
            if (elemStructures.OfType<Field>().FirstOrDefault() is Field f)
            {
                var vs = new MatrixSpace(matrix, f);
                structures.Add(vs);
            }
        }
        else if (type is SetType set)
        {
            structures.Add(new Magma(set.ElementType));
            var elemStructures = Classify(set.ElementType);
            structures.AddRange(elemStructures);
        }

        return structures;
    }

    private static IReadOnlyList<AlgebraicStructure> ClassifyScalar(ScalarType scalar)
    {
        var structures = new List<AlgebraicStructure>();

        if (scalar is IntegerType or TypedInteger)
        {
            structures.Add(new IntegralDomain(scalar));
            structures.Add(new Ring(scalar, isCommutative: true));
        }
        else if (scalar is RationalType)
        {
            structures.Add(new Field(scalar));
            structures.Add(new IntegralDomain(scalar));
            structures.Add(new Ring(scalar, isCommutative: true));
        }
        else if (scalar is RealType)
        {
            structures.Add(new Field(scalar));
            structures.Add(new IntegralDomain(scalar));
            structures.Add(new Ring(scalar, isCommutative: true));
            structures.Add(new OrderedField(scalar));
        }
        else if (scalar is ComplexType)
        {
            structures.Add(new Field(scalar));
            structures.Add(new IntegralDomain(scalar));
            structures.Add(new Ring(scalar, isCommutative: true));
        }

        return structures;
    }

    /// <summary>Returns the most specific (strongest) algebraic structure for a type.</summary>
    public static AlgebraicStructure? GetStrongest(MathType type)
    {
        var all = Classify(type);
        return all.MaxBy(s => StructureStrength(s.Kind));
    }

    private static int StructureStrength(AlgebraicStructureKind kind) => kind switch
    {
        AlgebraicStructureKind.OrderedField => 14,
        AlgebraicStructureKind.InnerProductSpace => 12,
        AlgebraicStructureKind.MetricSpace => 11,
        AlgebraicStructureKind.VectorSpace => 10,
        AlgebraicStructureKind.MatrixSpace => 9,
        AlgebraicStructureKind.Module => 8,
        AlgebraicStructureKind.Field => 7,
        AlgebraicStructureKind.IntegralDomain => 6,
        AlgebraicStructureKind.Ring => 5,
        AlgebraicStructureKind.AbelianGroup => 4,
        AlgebraicStructureKind.Group => 3,
        AlgebraicStructureKind.Monoid => 2,
        AlgebraicStructureKind.Semigroup => 1,
        AlgebraicStructureKind.Magma => 0,
        _ => -1,
    };
}

/// <summary>A field that is also ordered (has total order compatible with operations).</summary>
public sealed class OrderedField : Field
{
    /// <summary>Creates an ordered field.</summary>
    public OrderedField(MathType elementType) : base(elementType) { }

    /// <inheritdoc/>
    public override AlgebraicStructureKind Kind => AlgebraicStructureKind.OrderedField;

    /// <inheritdoc/>
    public override bool IsCommutative => true;

    /// <inheritdoc/>
    public override bool Equals(AlgebraicStructure? other) =>
        other is OrderedField of && of.ElementType.Equals(ElementType);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, ElementType);
}

/// <summary>A matrix space over a field (generalization of vector space for matrices).</summary>
public sealed class MatrixSpace : AlgebraicStructure
{
    /// <summary>The scalar field.</summary>
    public Field ScalarField { get; }

    /// <summary>Creates a matrix space.</summary>
    public MatrixSpace(MathType elementType, Field scalarField) : base(elementType)
    {
        ScalarField = scalarField;
    }

    /// <inheritdoc/>
    public override AlgebraicStructureKind Kind => AlgebraicStructureKind.MatrixSpace;

    /// <inheritdoc/>
    public override bool IsAssociative => true;

    /// <inheritdoc/>
    public override bool IsDistributive => true;

    /// <inheritdoc/>
    public override bool Equals(AlgebraicStructure? other) =>
        other is MatrixSpace ms && ms.ElementType.Equals(ElementType)
        && ms.ScalarField.Equals(ScalarField);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Kind, ElementType, ScalarField);
}
