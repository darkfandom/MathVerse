namespace MathVerse.Math.Parsing.Lexer;

/// <summary>
/// Public lexer interface for tokenizing mathematical source text.
/// Delegates to the internal lexer implementation in <see cref="ParsingFacade"/>.
/// </summary>
public sealed class Lexer
{
    private readonly string _source;
    private readonly LexerOptions _options;

    /// <summary>Initializes a lexer with the given source text and options.</summary>
    public Lexer(string source, LexerOptions? options = null)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
        _options = options ?? LexerOptions.Default;
    }

    /// <summary>Tokenizes the source text into an array of tokens.</summary>
    public Token[] Tokenize() => ParsingFacade.Tokenize(_source, _options);

    /// <summary>Tokenizes the source text into a stream.</summary>
    public TokenStream TokenizeToStream() => new(Tokenize());

    /// <summary>Static convenience method to tokenize source text.</summary>
    public static Token[] Tokenize(string source, LexerOptions? options = null) =>
        ParsingFacade.Tokenize(source, options);
}

/// <summary>
/// Provides sequential access to a pre-tokenized array.
/// </summary>
public sealed class TokenStream
{
    private readonly Token[] _tokens;
    private int _position;

    /// <summary>Initializes a token stream.</summary>
    public TokenStream(Token[] tokens)
    {
        _tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
    }

    /// <summary>Gets the current token.</summary>
    public Token Current => _position < _tokens.Length ? _tokens[_position] : Token.Eof(TokenPosition.Start);

    /// <summary>Peeks at the next token without consuming.</summary>
    public Token Peek() => (_position + 1) < _tokens.Length ? _tokens[_position + 1] : Token.Eof(TokenPosition.Start);

    /// <summary>Reads and advances to the next token.</summary>
    public Token Read()
    {
        var t = Current;
        if (_position < _tokens.Length) _position++;
        return t;
    }

    /// <summary>Gets whether the stream is at the end.</summary>
    public bool IsAtEnd => _position >= _tokens.Length;

    /// <summary>Gets the current position index.</summary>
    public int Position => _position;
}
