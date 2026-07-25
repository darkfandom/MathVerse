namespace MathVerse.Math.HPC.GpuBackend;

public readonly record struct GpuWorkSize(ulong X, ulong Y, ulong Z)
{
    public static readonly GpuWorkSize Empty = new(0, 0, 0);
    
    public static GpuWorkSize One => new(1, 1, 1);
    
    public static GpuWorkSize X1D(ulong x) => new(x, 1, 1);
    public static GpuWorkSize XY2D(ulong x, ulong y) => new(x, y, 1);
    public static GpuWorkSize XYZ3D(ulong x, ulong y, ulong z) => new(x, y, z);
    
    public ulong TotalSize => X * Y * Z;
    
    public bool IsEmpty => X == 0 || Y == 0 || Z == 0;
    public bool Is1D => Y == 1 && Z == 1;
    public bool Is2D => Z == 1 && !Is1D;
    public bool Is3D => !Is1D && !Is2D;
    
    public GpuWorkSize Divide(GpuWorkSize divisor)
    {
        if (divisor.X == 0 || divisor.Y == 0 || divisor.Z == 0)
            throw new DivideByZeroException("Work size divisor cannot have zero dimensions");
        return new GpuWorkSize(
            (X + divisor.X - 1) / divisor.X,
            (Y + divisor.Y - 1) / divisor.Y,
            (Z + divisor.Z - 1) / divisor.Z
        );
    }
    
    public GpuWorkSize Multiply(GpuWorkSize multiplier) => new(X * multiplier.X, Y * multiplier.Y, Z * multiplier.Z);
    public GpuWorkSize Min(GpuWorkSize other) => new(ulong.Min(X, other.X), ulong.Min(Y, other.Y), ulong.Min(Z, other.Z));
    public GpuWorkSize Max(GpuWorkSize other) => new(ulong.Max(X, other.X), ulong.Max(Y, other.Y), ulong.Max(Z, other.Z));
    
    public override string ToString() => Is1D ? $"({X})" : Is2D ? $"({X}, {Y})" : $"({X}, {Y}, {Z})";
    
    public static implicit operator GpuWorkSize((ulong X, ulong Y, ulong Z) tuple) => new(tuple.X, tuple.Y, tuple.Z);
    public static implicit operator (ulong X, ulong Y, ulong Z)(GpuWorkSize ws) => (ws.X, ws.Y, ws.Z);
}