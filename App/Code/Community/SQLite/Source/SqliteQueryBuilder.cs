using System.Collections;
using System.Data;
using System.Reflection;
using LLE.Kernel.DataQL.Ast;
using Microsoft.Data.Sqlite;

namespace LLE.SQLiteAdapter;

internal sealed record SqlQueryResult(string Sql, List<SqliteParameter> Parameters);

internal sealed class SqliteQueryBuilder : IAstVisitor<SqlQueryResult>
{
    private int _paramIndex = 0;
    private readonly List<SqliteParameter> _parameters = [];

    public SqlQueryResult Visit(ReadQueryNode node)
    {
        _paramIndex = 0;
        _parameters.Clear();

        var sql = $"SELECT * FROM [{node.TableName}]";

        if (node.Where is { } filter)
            sql += $" WHERE {BuildWhereClause(filter)}";

        sql += ';';
        return new SqlQueryResult(sql, [.. _parameters]);
    }

    public SqlQueryResult Visit(WriteQueryNode node)
    {
        _paramIndex = 0;
        _parameters.Clear();

        var (columns, values) = ExtractPayload(node.Payload);

        if (node.Where is null)
        {
            var cols = string.Join(", ", columns.Select(QuoteName));
            var prms = string.Join(", ", columns.Select((_, i) => AddParameter(values[i])));
            var sql = $"INSERT INTO [{node.TableName}] ({cols}) VALUES ({prms});";
            return new SqlQueryResult(sql, [.. _parameters]);
        }

        var setClauses = string.Join(", ", columns.Select((c, i) => $"{QuoteName(c)} = {AddParameter(values[i])}"));
        var updateSql = $"UPDATE [{node.TableName}] SET {setClauses} WHERE {BuildWhereClause(node.Where)};";
        return new SqlQueryResult(updateSql, [.. _parameters]);
    }

    public SqlQueryResult Visit(DeleteQueryNode node)
    {
        _paramIndex = 0;
        _parameters.Clear();

        var sql = $"DELETE FROM [{node.TableName}]";

        if (node.Where is { } filter)
            sql += $" WHERE {BuildWhereClause(filter)}";

        sql += ';';
        return new SqlQueryResult(sql, [.. _parameters]);
    }

    public SqlQueryResult Visit(FilterNode node)
    {
        _paramIndex = 0;
        _parameters.Clear();

        var sql = BuildFilterClause(node);
        return new SqlQueryResult(sql, [.. _parameters]);
    }

    private string BuildWhereClause(AstNode node)
    {
        if (node is FilterNode filter)
            return BuildFilterClause(filter);

        return node.Accept(this).Sql;
    }

    private string BuildFilterClause(FilterNode node)
    {
        if (node.Operator is FilterOperator.In or FilterOperator.NotIn)
            return BuildInClause(node);

        var op = MapOperator(node.Operator);
        return $"{QuoteName(node.ColumnName)} {op} {AddParameter(node.Value)}";
    }

    private string BuildInClause(FilterNode node)
    {
        var op = node.Operator == FilterOperator.In ? "IN" : "NOT IN";

        if (node.Value is not IEnumerable values)
            throw new InvalidOperationException($"Value for {op} filter must be a collection.");

        var prms = values.Cast<object?>().Select(AddParameter);
        return $"{QuoteName(node.ColumnName)} {op} ({string.Join(", ", prms)})";
    }

    private string AddParameter(object? value)
    {
        var name = $"@p{_paramIndex++}";
        _parameters.Add(new SqliteParameter(name, value ?? DBNull.Value));
        return name;
    }

    private static string QuoteName(string name) => $"[{name}]";

    private static string MapOperator(FilterOperator op) => op switch
    {
        FilterOperator.Equals => "=",
        FilterOperator.NotEquals => "!=",
        FilterOperator.LessThan => "<",
        FilterOperator.LessThanOrEquals => "<=",
        FilterOperator.GreaterThan => ">",
        FilterOperator.GreaterThanOrEquals => ">=",
        FilterOperator.Like => "LIKE",
        FilterOperator.NotLike => "NOT LIKE",
        _ => throw new InvalidOperationException($"Unknown filter operator: {op}")
    };

    private static (List<string> columns, List<object?> values) ExtractPayload(object payload)
    {
        if (payload is IDictionary<string, object?> dict)
            return ([.. dict.Keys], [.. dict.Values]);

        var props = payload.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
        return (props.Select(static p => p.Name).ToList(),
                props.Select(static p => p.GetValue(p)).ToList());
    }
}
