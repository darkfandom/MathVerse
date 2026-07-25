namespace MathVerse.Math.Parsing;

/// <summary>
/// High-level API entry point for the math parsing pipeline.
/// Provides static convenience methods that tie together Lexing, Parsing, and Expression conversion.
/// </summary>
public sealed class ParsingFacade
{
    /// <summary>
    /// Tokenizes the source text into an array of tokens.
    /// </summary>
    /// <param name="source">The source text to tokenize.</param>
    /// <param name="options">Optional lexer configuration.</param>
    /// <returns>An array of tokens produced by the lexer.</returns>
    public static Token[] Tokenize(string source, LexerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        var lexer = new MathLexer(source, options ?? LexerOptions.Default);
        return lexer.Tokenize();
    }

    /// <summary>
    /// Parses the source text into a <see cref="ParserResult"/> containing the syntax tree.
    /// </summary>
    /// <param name="source">The source text to parse.</param>
    /// <param name="options">Optional parser configuration.</param>
    /// <returns>A parser result containing the syntax tree and diagnostics.</returns>
    public static ParserResult Parse(string source, ParserOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        var opts = options ?? ParserOptions.Default;
        var tokens = Tokenize(source, opts.LexerOptions);
        var parser = new MathParser(tokens, opts);
        return parser.ParseTopLevelResult();
    }

    /// <summary>
    /// Parses the source text and converts it directly to an <see cref="Expression"/>.
    /// </summary>
    /// <param name="source">The source text to parse.</param>
    /// <param name="options">Optional parser configuration.</param>
    /// <returns>The converted expression.</returns>
    public static Expression ParseExpression(string source, ParserOptions? options = null)
    {
        var result = Parse(source, options);
        if (!result.Success || result.Root is null)
            throw new InvalidOperationException($"Parsing failed with {result.Diagnostics.GetErrors().Length} error(s)");
        var converter = new Conversion.SyntaxToExpressionConverter();
        return converter.Convert(result.Root);
    }

    /// <summary>
    /// Parses the source text as an equation and converts it to an <see cref="Expression"/>.
    /// </summary>
    /// <param name="source">The source text to parse.</param>
    /// <param name="options">Optional parser configuration.</param>
    /// <returns>The converted equation expression.</returns>
    public static Expression ParseEquation(string source, ParserOptions? options = null) =>
        ParseExpression(source, options);

    /// <summary>
    /// Parses the source text into a full <see cref="SyntaxTree"/>.
    /// </summary>
    /// <param name="source">The source text to parse.</param>
    /// <param name="lexerOptions">Optional lexer configuration.</param>
    /// <param name="parserOptions">Optional parser configuration.</param>
    /// <returns>The syntax tree.</returns>
    public static SyntaxTree ParseSyntaxTree(string source, LexerOptions? lexerOptions = null, ParserOptions? parserOptions = null)
    {
        ArgumentNullException.ThrowIfNull(source);
        var lexerOpts = lexerOptions ?? LexerOptions.Default;
        var parserOpts = parserOptions ?? ParserOptions.Default;
        var tokens = Tokenize(source, lexerOpts);
        var parser = new MathParser(tokens, parserOpts);
        return parser.BuildSyntaxTree();
    }

    /// <summary>
    /// Converts an existing <see cref="SyntaxTree"/> to an <see cref="Expression"/>.
    /// </summary>
    /// <param name="tree">The syntax tree to convert.</param>
    /// <returns>The converted expression.</returns>
    public static Expression ConvertToExpression(SyntaxTree tree)
    {
        ArgumentNullException.ThrowIfNull(tree);
        var converter = new Conversion.SyntaxToExpressionConverter();
        return converter.ConvertSyntaxTree(tree);
    }

    // ─────────────────────────────────────────────────────────────
    //  Private Lexer
    // ─────────────────────────────────────────────────────────────

    private sealed class MathLexer
    {
        private readonly CharacterReader _reader;
        private readonly LexerOptions _options;
        private readonly List<Token> _tokens = [];

        public MathLexer(string source, LexerOptions options)
        {
            _reader = new CharacterReader(source);
            _options = options;
        }

        public Token[] Tokenize()
        {
            while (!_reader.IsAtEnd)
            {
                var ch = _reader.Peek();
                if (char.IsWhiteSpace(ch) || UnicodeMathSupport.IsUnicodeWhitespace(ch))
                {
                    if (!_options.SkipWhitespace)
                        _tokens.Add(new Token(TokenType.Whitespace, _reader.Read().ToString(), _reader.CurrentPosition, 1));
                    else
                        _reader.Advance();
                    continue;
                }
                if (ch == '/' && _reader.Peek(1) == '/') { SkipLineComment(); continue; }
                if (ch == '/' && _reader.Peek(1) == '*') { SkipBlockComment(); continue; }
                if (char.IsDigit(ch) || (ch == '.' && char.IsDigit(_reader.Peek(1)))) { ReadNumber(); continue; }
                if (char.IsLetter(ch) || ch == '_' || UnicodeMathSupport.IsGreekLetter(ch)) { ReadIdentifierOrKeyword(); continue; }
                ReadSymbol();
            }
            _tokens.Add(Token.Eof(_reader.CurrentPosition));
            return [.. _tokens];
        }

        private void ReadNumber()
        {
            var pos = _reader.CurrentPosition;
            var start = _reader.Position;
            var hasDot = false;
            while (!_reader.IsAtEnd)
            {
                var c = _reader.Peek();
                if (char.IsDigit(c)) _reader.Advance();
                else if (c == '.' && !hasDot) { hasDot = true; _reader.Advance(); }
                else break;
            }
            if (!_reader.IsAtEnd && (_reader.Peek() == 'e' || _reader.Peek() == 'E'))
            {
                hasDot = true;
                _reader.Advance();
                if (!_reader.IsAtEnd && (_reader.Peek() == '+' || _reader.Peek() == '-'))
                    _reader.Advance();
                while (!_reader.IsAtEnd && char.IsDigit(_reader.Peek()))
                    _reader.Advance();
            }
            var text = _reader.GetSubstring(start);
            if (double.TryParse(text, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var dv))
                _tokens.Add(new Token(TokenType.RealLiteral, text, pos, text.Length, dv));
            else if (hasDot)
                _tokens.Add(new Token(TokenType.RealLiteral, text, pos, text.Length, 0.0));
            else if (int.TryParse(text, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out var iv))
                _tokens.Add(new Token(TokenType.IntegerLiteral, text, pos, text.Length, iv));
            else
                _tokens.Add(new Token(TokenType.IntegerLiteral, text, pos, text.Length, 0));
        }

        private void ReadIdentifierOrKeyword()
        {
            var pos = _reader.CurrentPosition;
            var start = _reader.Position;
            while (!_reader.IsAtEnd && (char.IsLetterOrDigit(_reader.Peek()) || _reader.Peek() == '_'))
            {
                if (UnicodeMathSupport.IsSuperScriptDigit(_reader.Peek()) || UnicodeMathSupport.IsSubScriptDigit(_reader.Peek()))
                    break;
                _reader.Advance();
            }
            var text = _reader.GetSubstring(start);
            var type = MapIdentifierToTokenType(text);
            object? val = type == TokenType.ConstantPi ? "pi"
                        : type == TokenType.ConstantE ? "e"
                        : type == TokenType.ConstantI ? "i"
                        : null;
            _tokens.Add(new Token(type, text, pos, text.Length, val));
        }

        private static TokenType MapIdentifierToTokenType(string text) => text switch
        {
            "true" => TokenType.KeywordTrue, "false" => TokenType.KeywordFalse,
            "fn" => TokenType.KeywordFn, "if" => TokenType.KeywordIf,
            "then" => TokenType.KeywordThen, "else" => TokenType.KeywordElse,
            "elif" => TokenType.KeywordElif, "let" => TokenType.KeywordLet,
            "in" => TokenType.KeywordIn, "where" => TokenType.KeywordWhere,
            "piecewise" => TokenType.KeywordPiecewise, "lim" => TokenType.Limit,
            "sin" => TokenType.FuncSin, "cos" => TokenType.FuncCos, "tan" => TokenType.FuncTan,
            "asin" => TokenType.FuncAsin, "acos" => TokenType.FuncAcos, "atan" => TokenType.FuncAtan,
            "sinh" => TokenType.FuncSinh, "cosh" => TokenType.FuncCosh, "tanh" => TokenType.FuncTanh,
            "ln" => TokenType.FuncLn, "log" => TokenType.FuncLog, "log10" => TokenType.FuncLog10,
            "exp" => TokenType.FuncExp, "sqrt" => TokenType.FuncSqrt, "cbrt" => TokenType.FuncCbrt,
            "abs" => TokenType.FuncAbs, "floor" => TokenType.FuncFloor, "ceil" => TokenType.FuncCeil,
            "round" => TokenType.FuncRound, "min" => TokenType.FuncMin, "max" => TokenType.FuncMax,
            "det" => TokenType.FuncDet, "mod" => TokenType.FuncMod,
            "pi" => TokenType.ConstantPi, "e" => TokenType.ConstantE, "i" => TokenType.ConstantI,
            _ => TokenType.Identifier
        };

        private void ReadSymbol()
        {
            var pos = _reader.CurrentPosition;
            var ch = _reader.Read();
            switch (ch)
            {
                case '+': Add(_reader.Match('=') ? TokenType.PlusEquals : TokenType.Plus, pos); break;
                case '-': Add(_reader.Match('=') ? TokenType.MinusEquals : TokenType.Minus, pos); break;
                case '*': Add(_reader.Match('=') ? TokenType.StarEquals : TokenType.Star, pos); break;
                case '/': Add(TokenType.Slash, pos); break;
                case '%': Add(TokenType.Percent, pos); break;
                case '^': Add(TokenType.Caret, pos); break;
                case '=': Add(_reader.Match('=') ? TokenType.EqualsEquals : TokenType.Equals, pos); break;
                case '!': Add(_reader.Match('=') ? TokenType.NotEquals : TokenType.Exclamation, pos); break;
                case '<': Add(_reader.Match('=') ? TokenType.LessThanOrEqual : TokenType.LessThan, pos); break;
                case '>': Add(_reader.Match('=') ? TokenType.GreaterThanOrEqual : TokenType.GreaterThan, pos); break;
                case '&': if (_reader.Match('&')) Add(TokenType.AmpersandAmpersand, pos); break;
                case '|': Add(_reader.Match('|') ? TokenType.PipePipe : TokenType.Pipe, pos); break;
                case '(': Add(TokenType.OpenParen, pos); break;
                case ')': Add(TokenType.CloseParen, pos); break;
                case '[': Add(TokenType.OpenBracket, pos); break;
                case ']': Add(TokenType.CloseBracket, pos); break;
                case '{': Add(TokenType.OpenBrace, pos); break;
                case '}': Add(TokenType.CloseBrace, pos); break;
                case ',': Add(TokenType.Comma, pos); break;
                case ';': Add(TokenType.Semicolon, pos); break;
                case ':': Add(_reader.Match('=') ? TokenType.Equals : TokenType.Colon, pos); break;
                case '.': Add(_reader.Match('.') ? TokenType.DotDot : TokenType.Dot, pos); break;
                case '"': ReadString(pos); break;
                default:
                    if (UnicodeMathSupport.TryGetSymbolTokenType(ch, out var symType))
                    {
                        _tokens.Add(new Token(symType, ch.ToString(), pos, 1));
                    }
                    else
                    {
                        var remaining = _reader.GetRemaining();
                        var combined = ch.ToString() + (remaining.Length > 0 ? remaining[0].ToString() : "");
                        if (UnicodeMathSupport.TryGetMultiCharSymbol(combined, out var mt, out var ml))
                        {
                            _reader.Advance(ml - 1);
                            _tokens.Add(new Token(mt, _reader.GetSubstring(pos.Offset), pos, ml));
                        }
                        else
                            _tokens.Add(new Token(TokenType.Unknown, ch.ToString(), pos, 1));
                    }
                    break;
            }
        }

        private void ReadString(TokenPosition pos)
        {
            var start = _reader.Position;
            var sb = new StringBuilder();
            while (!_reader.IsAtEnd && _reader.Peek() != '"')
            {
                if (_reader.Peek() == '\\') { _reader.Advance(); if (!_reader.IsAtEnd) { var e = _reader.Read(); sb.Append(e == 'n' ? '\n' : e == 't' ? '\t' : e); } }
                else sb.Append(_reader.Read());
            }
            if (!_reader.IsAtEnd) _reader.Advance();
            var text = _reader.GetSubstring(start);
            _tokens.Add(new Token(TokenType.StringLiteral, text, pos, text.Length, sb.ToString()));
        }

        private void SkipLineComment()
        {
            _reader.Advance(2);
            while (!_reader.IsAtEnd && _reader.Peek() != '\n') _reader.Advance();
        }

        private void SkipBlockComment()
        {
            _reader.Advance(2);
            while (!_reader.IsAtEnd) { if (_reader.Peek() == '*' && _reader.Peek(1) == '/') { _reader.Advance(2); return; } _reader.Advance(); }
        }

        private void Add(TokenType type, TokenPosition pos)
        {
            var lex = _reader.GetSubstring(pos.Offset, pos.Offset + (type == TokenType.Equals && _tokens.Count > 0 ? 2 : 1));
            _tokens.Add(new Token(type, lex, pos, lex.Length));
        }
    }

    // ─────────────────────────────────────────────────────────────
    //  Private Parser
    // ─────────────────────────────────────────────────────────────

    private sealed class MathParser
    {
        private readonly Token[] _tokens;
        private readonly ParserOptions _options;
        private readonly DiagnosticBag _diagnostics = new();
        private int _pos;

        public MathParser(Token[] tokens, ParserOptions options) { _tokens = tokens; _options = options; }

        private Token Current() => _pos < _tokens.Length ? _tokens[_pos] : Token.Eof(TokenPosition.Start);
        private Token Peek(int offset = 0) => (_pos + offset) < _tokens.Length ? _tokens[_pos + offset] : Token.Eof(TokenPosition.Start);

        private Token Advance()
        {
            var tok = Current();
            if (_pos < _tokens.Length) _pos++;
            return tok;
        }

        private Token Expect(TokenType type)
        {
            if (Current().Kind != type)
                throw new InvalidOperationException($"Expected {type} but found {Current().Kind} ('{Current().Lexeme}')");
            return Advance();
        }

        public ParserResult ParseTopLevelResult()
        {
            try
            {
                var root = ParseTopLevel();
                var tree = new SyntaxTree(root, [], [], _diagnostics);
                return ParserResult.Succeeded(tree, _diagnostics);
            }
            catch (InvalidOperationException ex)
            {
                _diagnostics.AddError("MV0001", ex.Message, 1, 1, 0);
                var fb = new LiteralExpressionSyntax(new SyntaxToken(SyntaxKind.IntegerLiteralToken, 0, "0", 0));
                var tree = new SyntaxTree(fb, [], [], _diagnostics);
                return ParserResult.Failed(tree, _diagnostics);
            }
        }

        public SyntaxTree BuildSyntaxTree()
        {
            try
            {
                var root = ParseTopLevel();
                return new SyntaxTree(root, [], [], _diagnostics);
            }
            catch (InvalidOperationException)
            {
                var fb = new LiteralExpressionSyntax(new SyntaxToken(SyntaxKind.IntegerLiteralToken, 0, "0", 0));
                return new SyntaxTree(fb, [], [], _diagnostics);
            }
        }

        private ExpressionSyntax ParseTopLevel()
        {
            var expr = ParseExpression(0);
            while (Current().Kind == TokenType.Semicolon)
            {
                Advance();
                if (Current().Kind == TokenType.Eof) break;
                expr = ParseExpression(0);
            }
            return expr;
        }

        private ExpressionSyntax ParseExpression(int minPrec)
        {
            var left = ParseUnary();
            while (true)
            {
                var tok = Current();
                if (tok.Kind == TokenType.Equals)
                {
                    if (_options.AllowEquations)
                    {
                        var eqPos = tok.Position.Offset;
                        Advance();
                        var right = ParseExpression(1);
                        left = new EquationExpressionSyntax(left, new SyntaxToken(SyntaxKind.EqualsToken, eqPos, "=", null), right);
                    }
                    else if (_options.AllowAssignments)
                    {
                        var eqPos = tok.Position.Offset;
                        Advance();
                        var right = ParseExpression(1);
                        left = new AssignmentExpressionSyntax(left, new SyntaxToken(SyntaxKind.EqualsToken, eqPos, "=", null), right);
                    }
                    else break;
                    continue;
                }
                var prec = GetBinaryPrecedence(tok.Kind);
                if (prec < minPrec) break;
                var opKind = MapTokenToSyntaxKind(tok.Kind);
                var opTok = new SyntaxToken(opKind, tok.Position.Offset, tok.Lexeme, null);
                Advance();
                var rhs = ParseExpression(prec + 1);
                left = new BinaryExpressionSyntax(left, opTok, rhs);
            }
            return left;
        }

        private ExpressionSyntax ParseUnary()
        {
            var tok = Current();
            if (tok.Kind == TokenType.Minus)
            {
                var p = tok.Position.Offset;
                Advance();
                return new UnaryExpressionSyntax(new SyntaxToken(SyntaxKind.MinusToken, p, "-", null), ParseUnary(), true);
            }
            if (tok.Kind == TokenType.Plus) { Advance(); return ParseUnary(); }
            if (tok.Kind == TokenType.Exclamation)
            {
                var p = tok.Position.Offset;
                Advance();
                return new UnaryExpressionSyntax(new SyntaxToken(SyntaxKind.ExclamationToken, p, "!", null), ParseUnary(), true);
            }
            if (tok.Kind == TokenType.Negation)
            {
                var p = tok.Position.Offset;
                Advance();
                return new UnaryExpressionSyntax(new SyntaxToken(SyntaxKind.NegationToken, p, "\u00AC", null), ParseUnary(), true);
            }
            return ParsePostfix();
        }

        private ExpressionSyntax ParsePostfix()
        {
            var expr = ParsePrimary();
            while (true)
            {
                var tok = Current();
                if (tok.Kind == TokenType.Exclamation)
                {
                    var p = tok.Position.Offset;
                    Advance();
                    expr = new PostfixExpressionSyntax(expr, new SyntaxToken(SyntaxKind.ExclamationToken, p, "!", null));
                }
                else if (tok.Kind == TokenType.Transpose)
                {
                    var p = tok.Position.Offset;
                    Advance();
                    expr = new PostfixExpressionSyntax(expr, new SyntaxToken(SyntaxKind.TransposeToken, p, "\u1D40", null));
                }
                else if (tok.Kind == TokenType.Inverse)
                {
                    var p = tok.Position.Offset;
                    Advance();
                    expr = new PostfixExpressionSyntax(expr, new SyntaxToken(SyntaxKind.InverseToken, p, "\u207B\u00B9", null));
                }
                else break;
            }
            return expr;
        }

        private ExpressionSyntax ParsePrimary()
        {
            var tok = Current();
            if (tok.Kind == TokenType.OpenParen) return ParseParenOrTuple();
            if (tok.Kind == TokenType.OpenBracket) return ParseVector();
            if (tok.Kind == TokenType.OpenBrace) return ParseSet();
            if (IsFunctionToken(tok.Kind)) return ParseFunctionCall();
            if (tok.Kind == TokenType.KeywordIf) return ParseConditional();
            if (tok.Kind == TokenType.KeywordPiecewise) return ParsePiecewise();
            if (tok.Kind == TokenType.KeywordFn) return ParseLambda();
            if (tok.Kind == TokenType.Summation) return ParseSummation();
            if (tok.Kind == TokenType.Product) return ParseProductNode();
            if (tok.Kind == TokenType.Integral) return ParseIntegral();
            if (tok.Kind == TokenType.Differential || tok.Kind == TokenType.Partial) return ParseDerivative();
            if (tok.Kind == TokenType.Limit) return ParseLimitNode();
            if (IsLiteralToken(tok.Kind)) return ParseLiteral();
            if (tok.Kind == TokenType.Identifier) return ParseIdentifier();
            throw new InvalidOperationException($"Unexpected token: {tok.Kind} ('{tok.Lexeme}')");
        }

        private ExpressionSyntax ParseLiteral()
        {
            var tok = Advance();
            var kind = tok.Kind switch
            {
                TokenType.IntegerLiteral => SyntaxKind.IntegerLiteralToken,
                TokenType.RealLiteral => SyntaxKind.RealLiteralToken,
                TokenType.ConstantPi or TokenType.ConstantE or TokenType.ConstantI or TokenType.ConstantInfinity => SyntaxKind.RealLiteralToken,
                TokenType.KeywordTrue => SyntaxKind.TrueKeyword,
                TokenType.KeywordFalse => SyntaxKind.FalseKeyword,
                _ => SyntaxKind.IntegerLiteralToken
            };
            object? val = tok.Kind switch
            {
                TokenType.IntegerLiteral => tok.Value,
                TokenType.RealLiteral => tok.Value,
                TokenType.ConstantPi => "pi",
                TokenType.ConstantE => "e",
                TokenType.ConstantI => "i",
                TokenType.ConstantInfinity => "infinity",
                TokenType.KeywordTrue => (object?)true,
                TokenType.KeywordFalse => (object?)false,
                _ => tok.Value
            };
            return new LiteralExpressionSyntax(new SyntaxToken(kind, tok.Position.Offset, tok.Lexeme, val));
        }

        private ExpressionSyntax ParseIdentifier()
        {
            var tok = Advance();
            return new IdentifierNameSyntax(new SyntaxToken(SyntaxKind.IdentifierToken, tok.Position.Offset, tok.Lexeme, tok.Lexeme));
        }

        private ExpressionSyntax ParseFunctionCall()
        {
            var nameTok = Advance();
            var nameKind = MapFuncTokenToSyntaxKind(nameTok.Kind);
            var openTok = Expect(TokenType.OpenParen);
            var args = new List<ExpressionSyntax>();
            if (Current().Kind != TokenType.CloseParen)
            {
                args.Add(ParseExpression(0));
                while (Current().Kind == TokenType.Comma) { Advance(); args.Add(ParseExpression(0)); }
            }
            var closeTok = Expect(TokenType.CloseParen);
            return new FunctionCallExpressionSyntax(
                new SyntaxToken(nameKind, nameTok.Position.Offset, nameTok.Lexeme, nameTok.Lexeme),
                new SyntaxToken(SyntaxKind.OpenParenToken, openTok.Position.Offset, "(", null),
                args,
                new SyntaxToken(SyntaxKind.CloseParenToken, closeTok.Position.Offset, ")", null));
        }

        private ExpressionSyntax ParseParenOrTuple()
        {
            var openTok = Advance();
            var openSyn = new SyntaxToken(SyntaxKind.OpenParenToken, openTok.Position.Offset, "(", null);
            if (Current().Kind == TokenType.CloseParen)
            {
                var closeTok = Advance();
                return new TupleExpressionSyntax(openSyn, [], new SyntaxToken(SyntaxKind.CloseParenToken, closeTok.Position.Offset, ")", null));
            }
            var first = ParseExpression(0);
            if (Current().Kind != TokenType.Comma)
            {
                var closeParen = Expect(TokenType.CloseParen);
                return new ParenthesizedExpressionSyntax(openSyn, first, new SyntaxToken(SyntaxKind.CloseParenToken, closeParen.Position.Offset, ")", null));
            }
            var elems = new List<ExpressionSyntax> { first };
            while (Current().Kind == TokenType.Comma) { Advance(); elems.Add(ParseExpression(0)); }
            var close = Expect(TokenType.CloseParen);
            return new TupleExpressionSyntax(openSyn, elems, new SyntaxToken(SyntaxKind.CloseParenToken, close.Position.Offset, ")", null));
        }

        private ExpressionSyntax ParseVector()
        {
            var openTok = Advance();
            var openSyn = new SyntaxToken(SyntaxKind.OpenBracketToken, openTok.Position.Offset, "[", null);
            var elems = new List<ExpressionSyntax>();
            if (Current().Kind != TokenType.CloseBracket)
            {
                elems.Add(ParseExpression(0));
                while (Current().Kind == TokenType.Comma) { Advance(); elems.Add(ParseExpression(0)); }
            }
            var closeTok = Expect(TokenType.CloseBracket);
            return new VectorLiteralExpressionSyntax(openSyn, elems, new SyntaxToken(SyntaxKind.CloseBracketToken, closeTok.Position.Offset, "]", null));
        }

        private ExpressionSyntax ParseSet()
        {
            var openTok = Advance();
            var openSyn = new SyntaxToken(SyntaxKind.OpenBraceToken, openTok.Position.Offset, "{", null);
            var elems = new List<ExpressionSyntax>();
            if (Current().Kind != TokenType.CloseBrace)
            {
                elems.Add(ParseExpression(0));
                while (Current().Kind == TokenType.Comma) { Advance(); elems.Add(ParseExpression(0)); }
            }
            var closeTok = Expect(TokenType.CloseBrace);
            return new SetLiteralExpressionSyntax(openSyn, elems, new SyntaxToken(SyntaxKind.CloseBraceToken, closeTok.Position.Offset, "}", null));
        }

        private ExpressionSyntax ParseConditional()
        {
            var ifTok = Advance();
            var cond = ParseExpression(0);
            var thenTok = Expect(TokenType.KeywordThen);
            var thenBranch = ParseExpression(0);
            var elseTok = Expect(TokenType.KeywordElse);
            var elseBranch = ParseExpression(0);
            return new ConditionalExpressionSyntax(
                new SyntaxToken(SyntaxKind.IfKeyword, ifTok.Position.Offset, "if", null), cond,
                new SyntaxToken(SyntaxKind.ThenKeyword, thenTok.Position.Offset, "then", null), thenBranch,
                new SyntaxToken(SyntaxKind.ElseKeyword, elseTok.Position.Offset, "else", null), elseBranch);
        }

        private ExpressionSyntax ParsePiecewise()
        {
            var kwTok = Advance();
            var openBrace = Expect(TokenType.OpenBrace);
            var cases = new List<PiecewiseCaseSyntax>();
            while (Current().Kind != TokenType.CloseBrace && Current().Kind != TokenType.Eof)
            {
                var val = ParseExpression(0);
                var whenTok = Expect(TokenType.KeywordWhere);
                var cond = ParseExpression(0);
                cases.Add(new PiecewiseCaseSyntax(val, new SyntaxToken(SyntaxKind.WhereKeyword, whenTok.Position.Offset, "when", null), cond));
                if (Current().Kind == TokenType.Comma) Advance();
            }
            var closeBrace = Expect(TokenType.CloseBrace);
            return new PiecewiseExpressionSyntax(
                new SyntaxToken(SyntaxKind.PiecewiseKeyword, kwTok.Position.Offset, "piecewise", null),
                new SyntaxToken(SyntaxKind.OpenBraceToken, openBrace.Position.Offset, "{", null),
                cases,
                new SyntaxToken(SyntaxKind.CloseBraceToken, closeBrace.Position.Offset, "}", null));
        }

        private ExpressionSyntax ParseLambda()
        {
            var fnTok = Advance();
            var openParen = Expect(TokenType.OpenParen);
            var parameters = new List<IdentifierNameSyntax>();
            if (Current().Kind != TokenType.CloseParen)
            {
                var idTok = Advance();
                parameters.Add(new IdentifierNameSyntax(new SyntaxToken(SyntaxKind.IdentifierToken, idTok.Position.Offset, idTok.Lexeme, idTok.Lexeme)));
                while (Current().Kind == TokenType.Comma)
                {
                    Advance();
                    var pTok = Advance();
                    parameters.Add(new IdentifierNameSyntax(new SyntaxToken(SyntaxKind.IdentifierToken, pTok.Position.Offset, pTok.Lexeme, pTok.Lexeme)));
                }
            }
            var closeParen = Expect(TokenType.CloseParen);
            var arrowTok = Current();
            SyntaxToken arrowSyn;
            if (arrowTok.Kind == TokenType.Arrow)
            {
                Advance();
                arrowSyn = new SyntaxToken(SyntaxKind.ArrowToken, arrowTok.Position.Offset, "\u2192", null);
            }
            else
            {
                arrowSyn = new SyntaxToken(SyntaxKind.EqualsToken, 0, "=>", null);
                Advance();
            }
            var body = ParseExpression(0);
            return new LambdaExpressionSyntax(
                new SyntaxToken(SyntaxKind.FnKeyword, fnTok.Position.Offset, "fn", null),
                new SyntaxToken(SyntaxKind.OpenParenToken, openParen.Position.Offset, "(", null),
                parameters,
                new SyntaxToken(SyntaxKind.CloseParenToken, closeParen.Position.Offset, ")", null),
                arrowSyn, body);
        }

        private ExpressionSyntax ParseDerivative()
        {
            var diffTok = Advance();
            var varTok = Expect(TokenType.Identifier);
            var divSyn = new SyntaxToken(SyntaxKind.SlashToken, 0, "/", null);
            var fn = ParsePrimary();
            return new DerivativeExpressionSyntax(
                new SyntaxToken(diffTok.Kind == TokenType.Partial ? SyntaxKind.PartialToken : SyntaxKind.IntegerLiteralToken, diffTok.Position.Offset, diffTok.Lexeme, null),
                new SyntaxToken(SyntaxKind.IdentifierToken, varTok.Position.Offset, varTok.Lexeme, varTok.Lexeme),
                divSyn, fn);
        }

        private ExpressionSyntax ParseIntegral()
        {
            var intTok = Advance();
            var integrand = ParsePostfix();
            var diffTok = Expect(TokenType.Differential);
            var varTok = Expect(TokenType.Identifier);
            ExpressionSyntax? lower = null, upper = null;
            if (Current().Kind == TokenType.OpenBracket)
            {
                Advance();
                lower = ParseExpression(0);
                Expect(TokenType.Comma);
                upper = ParseExpression(0);
                Expect(TokenType.CloseBracket);
            }
            return new IntegralExpressionSyntax(
                new SyntaxToken(SyntaxKind.IntegralToken, intTok.Position.Offset, "\u222B", null),
                integrand,
                new SyntaxToken(SyntaxKind.IntegerLiteralToken, diffTok.Position.Offset, "d", null),
                new SyntaxToken(SyntaxKind.IdentifierToken, varTok.Position.Offset, varTok.Lexeme, varTok.Lexeme),
                lower, upper);
        }

        private ExpressionSyntax ParseSummation()
        {
            var sumTok = Advance();
            var varTok = Expect(TokenType.Identifier);
            var eqTok = Expect(TokenType.Equals);
            var lower = ParseExpression(0);
            Expect(TokenType.Comma);
            var upper = ParseExpression(0);
            var body = ParseExpression(0);
            return new SummationExpressionSyntax(
                new SyntaxToken(SyntaxKind.SummationToken, sumTok.Position.Offset, "\u2211", null),
                new SyntaxToken(SyntaxKind.IdentifierToken, varTok.Position.Offset, varTok.Lexeme, varTok.Lexeme),
                new SyntaxToken(SyntaxKind.EqualsToken, eqTok.Position.Offset, "=", null),
                lower,
                new SyntaxToken(SyntaxKind.CommaToken, 0, ",", null),
                upper, body);
        }

        private ExpressionSyntax ParseProductNode()
        {
            var prodTok = Advance();
            var varTok = Expect(TokenType.Identifier);
            var eqTok = Expect(TokenType.Equals);
            var lower = ParseExpression(0);
            Expect(TokenType.Comma);
            var upper = ParseExpression(0);
            var body = ParseExpression(0);
            return new ProductExpressionSyntax(
                new SyntaxToken(SyntaxKind.ProductToken, prodTok.Position.Offset, "\u220F", null),
                new SyntaxToken(SyntaxKind.IdentifierToken, varTok.Position.Offset, varTok.Lexeme, varTok.Lexeme),
                new SyntaxToken(SyntaxKind.EqualsToken, eqTok.Position.Offset, "=", null),
                lower,
                new SyntaxToken(SyntaxKind.CommaToken, 0, ",", null),
                upper, body);
        }

        private ExpressionSyntax ParseLimitNode()
        {
            var limTok = Advance();
            var body = ParseExpression(0);
            var arrowTok = Current();
            SyntaxToken arrowSyn = arrowTok.Kind == TokenType.Arrow
                ? new SyntaxToken(SyntaxKind.ArrowToken, arrowTok.Position.Offset, "\u2192", null)
                : new SyntaxToken(SyntaxKind.ArrowToken, arrowTok.Position.Offset, "->", null);
            Advance();
            var varTok = Expect(TokenType.Identifier);
            var target = ParseExpression(0);
            return new LimitExpressionSyntax(
                new SyntaxToken(SyntaxKind.LimitKeyword, limTok.Position.Offset, "lim", null),
                body, arrowSyn,
                new SyntaxToken(SyntaxKind.IdentifierToken, varTok.Position.Offset, varTok.Lexeme, varTok.Lexeme),
                target);
        }

        private static int GetBinaryPrecedence(TokenType type) => type switch
        {
            TokenType.Equals => 1,
            TokenType.AmpersandAmpersand or TokenType.Wedge => 2,
            TokenType.PipePipe or TokenType.Vee => 2,
            TokenType.EqualsEquals or TokenType.NotEquals or TokenType.LessThan
                or TokenType.GreaterThan or TokenType.LessThanOrEqual or TokenType.GreaterThanOrEqual
                or TokenType.ElementOf => 3,
            TokenType.Plus or TokenType.Minus or TokenType.Union => 4,
            TokenType.Star or TokenType.Slash or TokenType.Percent or TokenType.Intersection
                or TokenType.DotProduct or TokenType.CrossProduct => 5,
            TokenType.Caret => 6,
            TokenType.Compose => 5,
            _ => -1
        };

        private static bool IsBinaryOperator(TokenType type) =>
            GetBinaryPrecedence(type) >= 0 && type != TokenType.Equals;

        private static bool IsFunctionToken(TokenType type) => type switch
        {
            TokenType.FuncSin or TokenType.FuncCos or TokenType.FuncTan or TokenType.FuncAsin
                or TokenType.FuncAcos or TokenType.FuncAtan or TokenType.FuncSinh or TokenType.FuncCosh
                or TokenType.FuncTanh or TokenType.FuncLn or TokenType.FuncLog or TokenType.FuncLog10
                or TokenType.FuncExp or TokenType.FuncSqrt or TokenType.FuncCbrt or TokenType.FuncAbs
                or TokenType.FuncFloor or TokenType.FuncCeil or TokenType.FuncRound or TokenType.FuncMin
                or TokenType.FuncMax or TokenType.FuncDet or TokenType.FuncMod => true,
            _ => false
        };

        private static bool IsLiteralToken(TokenType type) => type switch
        {
            TokenType.IntegerLiteral or TokenType.RealLiteral or TokenType.ConstantPi
                or TokenType.ConstantE or TokenType.ConstantI or TokenType.ConstantInfinity
                or TokenType.KeywordTrue or TokenType.KeywordFalse => true,
            _ => false
        };

        private static SyntaxKind MapTokenToSyntaxKind(TokenType type) => type switch
        {
            TokenType.Plus => SyntaxKind.PlusToken,
            TokenType.Minus => SyntaxKind.MinusToken,
            TokenType.Star => SyntaxKind.StarToken,
            TokenType.Slash => SyntaxKind.SlashToken,
            TokenType.Percent => SyntaxKind.PercentToken,
            TokenType.Caret => SyntaxKind.CaretToken,
            TokenType.EqualsEquals => SyntaxKind.EqualsEqualsToken,
            TokenType.NotEquals => SyntaxKind.NotEqualsToken,
            TokenType.LessThan => SyntaxKind.LessThanToken,
            TokenType.GreaterThan => SyntaxKind.GreaterThanToken,
            TokenType.LessThanOrEqual => SyntaxKind.LessThanOrEqualToken,
            TokenType.GreaterThanOrEqual => SyntaxKind.GreaterThanOrEqualToken,
            TokenType.Wedge or TokenType.AmpersandAmpersand => SyntaxKind.WedgeToken,
            TokenType.Vee or TokenType.PipePipe => SyntaxKind.VeeToken,
            TokenType.ElementOf => SyntaxKind.ElementOfToken,
            TokenType.Union => SyntaxKind.UnionToken,
            TokenType.Intersection => SyntaxKind.IntersectionToken,
            TokenType.DotProduct => SyntaxKind.DotProductToken,
            TokenType.CrossProduct => SyntaxKind.CrossProductToken,
            TokenType.Compose => SyntaxKind.ComposeToken,
            _ => SyntaxKind.UnknownToken
        };

        private static SyntaxKind MapFuncTokenToSyntaxKind(TokenType type) => type switch
        {
            TokenType.FuncSin => SyntaxKind.SinKeyword,
            TokenType.FuncCos => SyntaxKind.CosKeyword,
            TokenType.FuncTan => SyntaxKind.TanKeyword,
            TokenType.FuncAsin => SyntaxKind.AsinKeyword,
            TokenType.FuncAcos => SyntaxKind.AcosKeyword,
            TokenType.FuncAtan => SyntaxKind.AtanKeyword,
            TokenType.FuncSinh => SyntaxKind.SinhKeyword,
            TokenType.FuncCosh => SyntaxKind.CoshKeyword,
            TokenType.FuncTanh => SyntaxKind.TanhKeyword,
            TokenType.FuncLn => SyntaxKind.LnKeyword,
            TokenType.FuncLog => SyntaxKind.LogKeyword,
            TokenType.FuncExp => SyntaxKind.ExpKeyword,
            TokenType.FuncSqrt => SyntaxKind.SqrtKeyword,
            TokenType.FuncAbs => SyntaxKind.AbsKeyword,
            TokenType.FuncFloor => SyntaxKind.FloorKeyword,
            TokenType.FuncCeil => SyntaxKind.CeilKeyword,
            TokenType.FuncRound => SyntaxKind.RoundKeyword,
            TokenType.FuncMin => SyntaxKind.MinKeyword,
            TokenType.FuncMax => SyntaxKind.MaxKeyword,
            TokenType.FuncDet => SyntaxKind.DetKeyword,
            TokenType.FuncMod => SyntaxKind.ModKeyword,
            _ => SyntaxKind.IdentifierToken
        };
    }
}
