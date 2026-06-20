namespace LLE.Kernel.DataQL.Tokeniser
{
    public static class TokenArrayBuilder
    {
        public static List<Token> Parse(string source)
        {
            var tokens = new List<Token>();

            for (var i = 0; i < source.Length;)
            {
                var c = source[i];

                if (char.IsWhiteSpace(c))
                {
                    i++;
                    continue;
                }

                switch (c)
                {
                    case '(':
                        tokens.Add(new Token(TokenKind.OpenParen, i) { Length = 1 });
                        i++;
                        continue;

                    case ')':
                        tokens.Add(new Token(TokenKind.CloseParen, i) { Length = 1 });
                        i++;
                        continue;

                    case ',':
                        tokens.Add(new Token(TokenKind.Comma, i) { Length = 1 });
                        i++;
                        continue;

                    case '.':
                        tokens.Add(new Token(TokenKind.Dot, i) { Length = 1 });
                        i++;
                        continue;

                    case ':':
                    {
                        var start = ++i;

                        while (i < source.Length &&
                               (char.IsLetterOrDigit(source[i]) || source[i] == '_'))
                        {
                            i++;
                        }

                        tokens.Add(new Token(TokenKind.Parameter, start)
                        {
                            Length = i - start
                        });

                        continue;
                    }

                    case '=':
                        tokens.Add(new Token(TokenKind.Operator, i) { Length = 1 });
                        i++;
                        continue;

                    case '>':
                    {
                        var length =
                            i + 1 < source.Length &&
                            source[i + 1] == '='
                                ? 2
                                : 1;

                        tokens.Add(new Token(TokenKind.Operator, i)
                        {
                            Length = length
                        });

                        i += length;
                        continue;
                    }

                    case '<':
                    {
                        var length =
                            i + 1 < source.Length &&
                            source[i + 1] == '='
                                ? 2
                                : 1;

                        tokens.Add(new Token(TokenKind.Operator, i)
                        {
                            Length = length
                        });

                        i += length;
                        continue;
                    }

                    case '!':
                    {
                        if (i + 1 >= source.Length || source[i + 1] != '=')
                            throw new Exception($"Unexpected character '!' at position {i}");

                        tokens.Add(new Token(TokenKind.Operator, i)
                        {
                            Length = 2
                        });

                        i += 2;
                        continue;
                    }

                    case '"':
                    {
                        var start = ++i;

                        while (i < source.Length && source[i] != '"')
                        {
                            i++;
                        }

                        if (i >= source.Length)
                            throw new Exception("Unterminated string literal.");

                        tokens.Add(new Token(TokenKind.Literal, start)
                        {
                            Length = i - start
                        });

                        i++; // skip closing quote
                        continue;
                    }
                }

                if (char.IsDigit(c))
                {
                    var start = i;

                    while (i < source.Length &&
                           (char.IsDigit(source[i]) || source[i] == '.'))
                    {
                        i++;
                    }

                    tokens.Add(new Token(TokenKind.Literal, start)
                    {
                        Length = i - start
                    });

                    continue;
                }

                if (char.IsLetter(c) || c == '_')
                {
                    var start = i;

                    while (i < source.Length &&
                           (char.IsLetterOrDigit(source[i]) || source[i] == '_'))
                    {
                        i++;
                    }

                    var length = i - start;
                    var span = source.AsSpan(start, length);

                    var kind =
                        span.Equals("and".AsSpan(), StringComparison.OrdinalIgnoreCase)
                            ? TokenKind.And
                        : span.Equals("or".AsSpan(), StringComparison.OrdinalIgnoreCase)
                            ? TokenKind.Or
                        : span.Equals("not".AsSpan(), StringComparison.OrdinalIgnoreCase)
                            ? TokenKind.Not
                        : span.Equals("in".AsSpan(), StringComparison.OrdinalIgnoreCase)
                            ? TokenKind.In
                        : span.Equals("like".AsSpan(), StringComparison.OrdinalIgnoreCase)
                            ? TokenKind.Like
                        : TokenKind.Identifier;

                    tokens.Add(new Token(kind, start)
                    {
                        Length = length
                    });

                    continue;
                }

                throw new Exception(
                    $"Unexpected character '{c}' at position {i}");
            }

            tokens.Add(new Token(TokenKind.EndOfFile, source.Length)
            {
                Length = 0
            });

            return tokens;
        }
    }
}