namespace MathVerse.Math.Performance.Hashing;

/// <summary>
/// Incremental hash builder that accumulates hash contributions from multiple sources.
/// </summary>
public sealed class HashBuilder
{
    private HashCode _hashCode;

    /// <summary>
    /// Initializes a new hash builder.
    /// </summary>
    public HashBuilder()
    {
        _hashCode = new HashCode();
    }

    /// <summary>
    /// Adds a value to the hash computation.
    /// </summary>
    /// <typeparam name="T">The type of value to hash.</typeparam>
    /// <param name="value">The value to include.</param>
    /// <returns>This <see cref="HashBuilder"/> for fluent chaining.</returns>
    public HashBuilder Add<T>(T value)
    {
        _hashCode.Add(value);
        return this;
    }

    /// <summary>
    /// Adds a byte span to the hash computation.
    /// </summary>
    /// <param name="data">The byte data to include.</param>
    /// <returns>This <see cref="HashBuilder"/> for fluent chaining.</returns>
    public HashBuilder AddBytes(ReadOnlySpan<byte> data)
    {
        _hashCode.AddBytes(data);
        return this;
    }

    /// <summary>
    /// Produces the final hash code.
    /// </summary>
    /// <returns>The computed hash code.</returns>
    public int ToHashCode() => _hashCode.ToHashCode();
}
