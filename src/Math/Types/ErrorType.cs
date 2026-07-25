namespace MathVerse.Math.Types;

/// <summary>Represents a type error (recovery type). Carries an optional message.</summary>
public sealed class ErrorType : MathType
{
    /// <summary>The singleton instance.</summary>
    public static readonly ErrorType Instance = new();

    private ErrorType() { }

    /// <inheritdoc/>
    public override TypeKind Kind => TypeKind.Error;

    /// <inheritdoc/>
    public override string Name => "⊥";

    /// <inheritdoc/>
    public override bool IsError => true;

    /// <inheritdoc/>
    public override bool Equals(MathType? other) => other is ErrorType;

    /// <inheritdoc/>
    public override int GetHashCode() => typeof(ErrorType).GetHashCode();
}
