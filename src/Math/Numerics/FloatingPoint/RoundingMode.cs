namespace MathVerse.Math.Numerics.FloatingPoint;

public enum RoundingMode : byte
{
    ToNearest = 0,
    TowardZero = 1,
    TowardPositive = 2,
    TowardNegative = 3,
    Bankers = 4
}