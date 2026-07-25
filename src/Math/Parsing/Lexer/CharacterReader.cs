namespace MathVerse.Math.Parsing.Lexer;

/// <summary>
/// Provides efficient character-by-character reading of source text.
/// </summary>
public sealed class CharacterReader
{
    private readonly string _source;
    private int _position;
    private int _line = 1;
    private int _column = 1;

    /// <summary>Initializes a character reader from a string.</summary>
    public CharacterReader(string source)
    {
        _source = source ?? throw new ArgumentNullException(nameof(source));
    }

    /// <summary>Gets the total length of the source.</summary>
    public int Length => _source.Length;

    /// <summary>Gets whether the reader has reached the end.</summary>
    public bool IsAtEnd => _position >= _source.Length;

    /// <summary>Gets the current 0-based offset.</summary>
    public int Position => _position;

    /// <summary>Gets the current 1-based line number.</summary>
    public int Line => _line;

    /// <summary>Gets the current 1-based column number.</summary>
    public int Column => _column;

    /// <summary>Gets the current position as a TokenPosition.</summary>
    public TokenPosition CurrentPosition => new(_line, _column, _position);

    /// <summary>Peeks at the current character without consuming it.</summary>
    public char Peek()
    {
        if (IsAtEnd) return '\0';
        return _source[_position];
    }

    /// <summary>Peeks at the character at the given offset from current position.</summary>
    public char Peek(int offset)
    {
        var idx = _position + offset;
        if (idx < 0 || idx >= _source.Length) return '\0';
        return _source[idx];
    }

    /// <summary>Reads and consumes the current character.</summary>
    public char Read()
    {
        if (IsAtEnd) return '\0';
        var c = _source[_position];
        _position++;
        if (c == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }
        return c;
    }

    /// <summary>Reads a specific number of characters.</summary>
    public string Read(int count)
    {
        var start = _position;
        var end = System.Math.Min(_position + count, _source.Length);
        var result = _source[start..end];
        for (var i = 0; i < result.Length; i++)
            Advance();
        return result;
    }

    /// <summary>Advances past the current character without returning it.</summary>
    public void Advance()
    {
        if (IsAtEnd) return;
        var c = _source[_position];
        _position++;
        if (c == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }
    }

    /// <summary>Advances n characters.</summary>
    public void Advance(int count)
    {
        for (var i = 0; i < count && !IsAtEnd; i++)
            Advance();
    }

    /// <summary>Matches the current character and advances if it matches.</summary>
    public bool Match(char expected)
    {
        if (IsAtEnd || _source[_position] != expected) return false;
        Advance();
        return true;
    }

    /// <summary>Matches any of the given characters and advances.</summary>
    public bool MatchAny(params char[] characters)
    {
        if (IsAtEnd) return false;
        var c = _source[_position];
        foreach (var expected in characters)
        {
            if (c == expected)
            {
                Advance();
                return true;
            }
        }
        return false;
    }

    /// <summary>Reads while the predicate matches.</summary>
    public string ReadWhile(Func<char, bool> predicate)
    {
        var start = _position;
        while (!IsAtEnd && predicate(_source[_position]))
            Advance();
        return _source[start.._position];
    }

    /// <summary>Reads until the predicate matches.</summary>
    public string ReadUntil(Func<char, bool> predicate)
    {
        return ReadWhile(c => !predicate(c));
    }

    /// <summary>Gets the substring from the given start to current position.</summary>
    public string GetSubstring(int start, int? end = null)
    {
        var e = end ?? _position;
        if (start < 0) start = 0;
        if (e > _source.Length) e = _source.Length;
        if (start > e) return string.Empty;
        return _source[start..e];
    }

    /// <summary>Gets the remaining source text from current position.</summary>
    public string GetRemaining() =>
        _position >= _source.Length ? string.Empty : _source[_position..];
}
