using System.Collections.Generic;
using LLE.Kernel.DataQL.Tokeniser;

namespace LLE.Kernel.DataQL.Ast;

public static class AstParser
{
    public static AstNode Parse(Type entityType, string query)
    {
        return Parse(entityType, query, null);
    }

    public static AstNode Parse(Type entityType, string query, IReadOnlyDictionary<string, object?>? parameters)
    {
        var tokens = TokenArrayBuilder.Parse(query);
        var parser = new Parser(tokens, query, parameters);
        var filterExpr = parser.ParseExpression();

        return new ReadQueryNode
        {
            TableName = entityType.Name,
            Where = filterExpr,
            EntityType = entityType,
            OperationKind = Enums.OperationKind.Read
        };
    }

    private sealed class Parser(List<Token> tokens, string source, IReadOnlyDictionary<string, object?>? parameters)
    {
        private readonly List<Token> _tokens = tokens;
        private readonly string _source = source;
        private readonly IReadOnlyDictionary<string, object?>? _parameters = parameters;
        private int _pos;

        private Token Current => _tokens[_pos];
        private TokenKind CurrentKind => Current.Kind;

        public AstNode? ParseExpression()
        {
            if (CurrentKind == TokenKind.EndOfFile)
                return null;

            var expr = ParseOrExpr();

            if (CurrentKind != TokenKind.EndOfFile)
                throw new Exception($"Unexpected token '{CurrentKind}' at position {Current.StartPosition}");

            return expr;
        }

        private AstNode ParseOrExpr()
        {
            var left = ParseAndExpr();

            while (CurrentKind == TokenKind.Or)
            {
                Consume();
                var right = ParseAndExpr();
                left = new LogicalNode
                {
                    Operator = LogicalOperator.Or,
                    Left = left,
                    Right = right
                };
            }

            return left;
        }

        private AstNode ParseAndExpr()
        {
            var left = ParseNotExpr();

            while (CurrentKind == TokenKind.And)
            {
                Consume();
                var right = ParseNotExpr();
                left = new LogicalNode
                {
                    Operator = LogicalOperator.And,
                    Left = left,
                    Right = right
                };
            }

            return left;
        }

        private AstNode ParseNotExpr()
        {
            if (CurrentKind == TokenKind.Not)
            {
                Consume();
                var operand = ParseNotExpr();
                return new UnaryNode
                {
                    Operator = UnaryOperator.Not,
                    Operand = operand
                };
            }

            return ParsePrimary();
        }

        private AstNode ParsePrimary()
        {
            if (CurrentKind == TokenKind.OpenParen)
            {
                Consume();
                var expr = ParseExpression();
                Expect(TokenKind.CloseParen);
                Consume();
                return expr!;
            }

            return ParseComparison();
        }

        private AstNode ParseComparison()
        {
            var field = ParseField();
            var op = ParseOperator();

            if (op is FilterOperator.In or FilterOperator.NotIn)
            {
                var values = ParseListValue();
                return new FilterNode
                {
                    ColumnName = field,
                    Operator = op,
                    Value = values
                };
            }

            var value = ParseValue();
            return new FilterNode
            {
                ColumnName = field,
                Operator = op,
                Value = value!
            };
        }

        private string ParseField()
        {
            var field = GetTokenText(Consume(TokenKind.Identifier));

            while (CurrentKind == TokenKind.Dot)
            {
                Consume();
                field += "." + GetTokenText(Consume(TokenKind.Identifier));
            }

            return field;
        }

        private FilterOperator ParseOperator()
        {
            if (CurrentKind == TokenKind.In)
            {
                Consume();
                return FilterOperator.In;
            }

            if (CurrentKind == TokenKind.Not)
            {
                Consume();

                if (CurrentKind == TokenKind.In)
                {
                    Consume();
                    return FilterOperator.NotIn;
                }

                if (CurrentKind == TokenKind.Like)
                {
                    Consume();
                    return FilterOperator.NotLike;
                }

                throw new Exception($"Expected 'in' or 'like' after 'not' at position {Current.StartPosition}");
            }

            if (CurrentKind == TokenKind.Like)
            {
                Consume();
                return FilterOperator.Like;
            }

            if (CurrentKind == TokenKind.Operator)
            {
                var opText = GetTokenText(Current);
                Consume();
                return opText switch
                {
                    "=" => FilterOperator.Equals,
                    "!=" => FilterOperator.NotEquals,
                    ">" => FilterOperator.GreaterThan,
                    "<" => FilterOperator.LessThan,
                    ">=" => FilterOperator.GreaterThanOrEquals,
                    "<=" => FilterOperator.LessThanOrEquals,
                    _ => throw new Exception($"Unknown operator '{opText}' at position {Current.StartPosition}")
                };
            }

            throw new Exception($"Expected operator at position {Current.StartPosition}");
        }

        private object? ParseValue()
        {
            if (CurrentKind == TokenKind.Parameter)
            {
                var paramName = GetTokenText(Current);
                Consume();

                if (_parameters is not null && _parameters.TryGetValue(paramName, out var value))
                    return value;

                throw new Exception($"Parameter ':{paramName}' was not provided. " +
                    "Ensure the query method has a matching parameter.");
            }

            if (CurrentKind == TokenKind.Literal)
                return ParseLiteral();

            if (CurrentKind == TokenKind.Identifier)
            {
                var text = GetTokenText(Current);
                if (text.Equals("true", StringComparison.OrdinalIgnoreCase))
                {
                    Consume();
                    return true;
                }
                if (text.Equals("false", StringComparison.OrdinalIgnoreCase))
                {
                    Consume();
                    return false;
                }
                if (text.Equals("null", StringComparison.OrdinalIgnoreCase))
                {
                    Consume();
                    return null;
                }
            }

            throw new Exception($"Expected value at position {Current.StartPosition}");
        }

        private object?[] ParseListValue()
        {
            Expect(TokenKind.OpenParen);
            Consume();

            var values = new List<object?>();

            if (CurrentKind != TokenKind.CloseParen)
            {
                values.Add(ParseValue());

                while (CurrentKind == TokenKind.Comma)
                {
                    Consume();
                    values.Add(ParseValue());
                }
            }

            Expect(TokenKind.CloseParen);
            Consume();

            return values.ToArray();
        }

        private object? ParseLiteral()
        {
            var text = GetTokenText(Current);
            Consume();

            if (int.TryParse(text, out int intVal))
                return intVal;

            if (text.Contains('.') && double.TryParse(text, out double dblVal))
                return dblVal;

            return text;
        }

        private string GetTokenText(Token token)
            => _source.Substring(token.StartPosition, token.Length);

        private Token Consume(TokenKind expected)
        {
            if (CurrentKind != expected)
                throw new Exception($"Expected {expected} but found {CurrentKind} at position {Current.StartPosition}");
            return Consume();
        }

        private Token Consume()
            => _tokens[_pos++];

        private void Expect(TokenKind kind)
        {
            if (CurrentKind != kind)
                throw new Exception($"Expected {kind} but found {CurrentKind} at position {Current.StartPosition}");
        }
    }
}
