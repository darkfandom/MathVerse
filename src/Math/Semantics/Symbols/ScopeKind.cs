namespace MathVerse.Math.Semantics.Symbols;

/// <summary>Categorizes scope kinds.</summary>
public enum ScopeKind
{
    /// <summary>The global/root scope.</summary>
    Global,
    /// <summary>A function body scope.</summary>
    Function,
    /// <summary>A lambda body scope.</summary>
    Lambda,
    /// <summary>A local block scope.</summary>
    Local,
    /// <summary>An imported scope.</summary>
    Imported,
    /// <summary>A namespace scope.</summary>
    Namespace,
}
