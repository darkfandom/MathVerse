namespace MathVerse.Math.Foundation.Domains;

public sealed class QuaternionDomain
{
    private static readonly Lazy<MathDomain> LazyInstance = new(() => new MathDomain
    {
        Name = "Quaternion",
        Kind = DomainKind.Quaternion,
        Parents = ImmutableArray<MathDomain>.Empty,
        DoublePredicate = _ => true,
        ComplexPredicate = _ => true
    });

    public static MathDomain Instance => LazyInstance.Value;
}
