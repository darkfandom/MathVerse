namespace MathVerse.Math.Foundation.Domains;

public sealed record MatrixDomain
{
    public MathDomain ElementDomain { get; init; }

    public int Rows { get; init; }

    public int Columns { get; init; }

    public MathDomain Domain { get; }

    public MatrixDomain(MathDomain elementDomain, int rows, int columns)
    {
        ElementDomain = elementDomain;
        Rows = rows;
        Columns = columns;
        Domain = new MathDomain
        {
            Name = $"{rows}x{columns} Matrix over {elementDomain.Name}",
            Kind = DomainKind.Matrix,
            Parents = ImmutableArray<MathDomain>.Empty,
            DoublePredicate = _ => false,
            ComplexPredicate = _ => false
        };
    }
}
