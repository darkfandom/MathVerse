namespace MathVerse.Math.Parsing.Diagnostics;

/// <summary>
/// Provides standardized diagnostic codes for the MathVerse parser.
/// </summary>
public static class DiagnosticCode
{
    /// <summary>Unexpected end of input.</summary>
    public const string UnexpectedEOF = "MV0001";

    /// <summary>Unexpected character.</summary>
    public const string UnexpectedCharacter = "MV0002";

    /// <summary>Unterminated string literal.</summary>
    public const string UnterminatedString = "MV0003";

    /// <summary>Unterminated block comment.</summary>
    public const string UnterminatedComment = "MV0004";

    /// <summary>Invalid number literal.</summary>
    public const string InvalidNumber = "MV0005";

    /// <summary>Invalid identifier.</summary>
    public const string InvalidIdentifier = "MV0006";

    /// <summary>Expected token but found another.</summary>
    public const string UnexpectedToken = "MV0010";

    /// <summary>Expected expression.</summary>
    public const string ExpectedExpression = "MV0011";

    /// <summary>Expected closing delimiter.</summary>
    public const string ExpectedClosingDelimiter = "MV0012";

    /// <summary>Unexpected token in expression.</summary>
    public const string UnexpectedTokenInExpression = "MV0013";

    /// <summary>Expected identifier.</summary>
    public const string ExpectedIdentifier = "MV0014";

    /// <summary>Expected operator.</summary>
    public const string ExpectedOperator = "MV0015";

    /// <summary>Missing argument list for function call.</summary>
    public const string ExpectedArgumentList = "MV0016";

    /// <summary>Invalid assignment target.</summary>
    public const string InvalidAssignmentTarget = "MV0020";

    /// <summary>Invalid equation syntax.</summary>
    public const string InvalidEquation = "MV0021";

    /// <summary>Invalid calculus syntax.</summary>
    public const string InvalidCalculusSyntax = "MV0030";

    /// <summary>Invalid integral bounds.</summary>
    public const string InvalidIntegralBounds = "MV0031";

    /// <summary>Invalid summation bounds.</summary>
    public const string InvalidSummationBounds = "MV0032";

    /// <summary>Invalid derivative syntax.</summary>
    public const string InvalidDerivativeSyntax = "MV0033";

    /// <summary>Invalid limit syntax.</summary>
    public const string InvalidLimitSyntax = "MV0034";

    /// <summary>Invalid matrix syntax.</summary>
    public const string InvalidMatrixSyntax = "MV0040";

    /// <summary>Invalid vector syntax.</summary>
    public const string InvalidVectorSyntax = "MV0041";

    /// <summary>Invalid set syntax.</summary>
    public const string InvalidSetSyntax = "MV0042";

    /// <summary>Invalid interval syntax.</summary>
    public const string InvalidIntervalSyntax = "MV0043";

    /// <summary>Invalid piecewise syntax.</summary>
    public const string InvalidPiecewiseSyntax = "MV0050";

    /// <summary>Invalid lambda syntax.</summary>
    public const string InvalidLambdaSyntax = "MV0051";

    /// <summary>Empty expression.</summary>
    public const string EmptyExpression = "MV0060";

    /// <summary>Ambiguous expression.</summary>
    public const string AmbiguousExpression = "MV0061";

    /// <summary>Implicit multiplication not enabled.</summary>
    public const string ImplicitMultiplicationDisabled = "MV0062";

    /// <summary>Unknown Unicode symbol.</summary>
    public const string UnknownUnicodeSymbol = "MV0070";
}
