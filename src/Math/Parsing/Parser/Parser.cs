namespace MathVerse.Math.Parsing.Parser;

/// <summary>
/// Public parser interface for transforming token streams into expression syntax trees.
/// Delegates to the internal parser implementation in <see cref="ParsingFacade"/>.
/// </summary>
public sealed class Parser
{
    private readonly string _source;
    private readonly ParserOptions _options;

    /// <summary>Initializes a parser with the given source text and options.</summary>
    public Parser(string source, ParserOptions? options = null)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _options = options ?? ParserOptions.Default;
    }

    /// <summary>Parses the source text into a <see cref="ParserResult"/>.</summary>
    public ParserResult ParseResult() => ParsingFacade.Parse(_source, _options);

    /// <summary>Parses the source text into a single expression.</summary>
    public ExpressionSyntax? ParseExpression()
    {
        var result = ParsingFacade.Parse(_source, _options);
        return result.Root;
    }

    /// <summary>Parses the source text and converts to an <see cref="Expression"/>.</summary>
    public Expression ParseToExpression() => ParsingFacade.ParseExpression(_source, _options);

    /// <summary>Parses the source text as an equation.</summary>
    public Expression ParseEquation() => ParsingFacade.ParseEquation(_source, _options);

    /// <summary>Parses semicolon-separated statements.</summary>
    public ParserResult ParseStatements() => ParsingFacade.Parse(_source, _options);

    /// <summary>Static convenience: parses source into a <see cref="ParserResult"/>.</summary>
    public static ParserResult Parse(string source, ParserOptions? options = null) =>
        ParsingFacade.Parse(source, options);

    /// <summary>Static convenience: parses source into an <see cref="Expression"/>.</summary>
    public static Expression ParseExpression(string source, ParserOptions? options = null) =>
        ParsingFacade.ParseExpression(source, options);

    /// <summary>Static convenience: parses source as an equation.</summary>
    public static Expression ParseEquation(string source, ParserOptions? options = null) =>
        ParsingFacade.ParseEquation(source, options);

    /// <summary>Static convenience: parses source into a <see cref="SyntaxTree"/>.</summary>
    public static SyntaxTree ParseSyntaxTree(string source, LexerOptions? lexerOptions = null, ParserOptions? parserOptions = null) =>
        ParsingFacade.ParseSyntaxTree(source, lexerOptions, parserOptions);
}
