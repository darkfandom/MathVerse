namespace MathVerse.Math.Foundation.Domains;

[Flags]
public enum DomainKind
{
    None = 0,
    Real = 1,
    Integer = 2,
    Natural = 4,
    Whole = 8,
    Rational = 16,
    Complex = 32,
    Quaternion = 64,
    Boolean = 128,
    FiniteField = 256,
    Vector = 512,
    Matrix = 1024,
    Tensor = 2048,
    Function = 4096,
    Set = 8192
}
