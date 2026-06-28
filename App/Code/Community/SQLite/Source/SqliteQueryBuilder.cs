using System.Data;
using System.Reflection;
using System.Text;
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

        var orderObj = (object?)node.OrderBy;
        switch (orderObj)
        {
            case List<SortOption> list when list.Count > 0:
                var sb = new StringBuilder();
                for (var i = 0; i < list.Count; i++)
                {
                    if (i > 0) sb.Append(", ");
                    sb.Append(QuoteName(list[i].Field));
                    sb.Append(list[i].Ascending ? " ASC" : " DESC");
                }
                sql += $" ORDER BY {sb}";
                break;

            case SortOption opt:
                sql += $" ORDER BY {QuoteName(opt.Field)} {(opt.Ascending ? "ASC" : "DESC")}";
                break;
        }

        if (node.Pagination is { } pagination)
        {
            var offset = (pagination.PageNo - 1) * pagination.Limit;
            sql += $" LIMIT {pagination.Limit} OFFSET {offset}";
        }

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
            var cols = JoinColumnNames(columns);
            var prms = JoinParameters(values);
            var sql = $"INSERT OR IGNORE INTO [{node.TableName}] ({cols}) VALUES ({prms});";
            return new SqlQueryResult(sql, [.. _parameters]);
        }

        var setSql = BuildSetClause(columns, values);
        var updateSql = $"UPDATE [{node.TableName}] SET {setSql} WHERE {BuildWhereClause(node.Where)};";
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

    public SqlQueryResult Visit(LogicalNode node)
    {
        _paramIndex = 0;
        _parameters.Clear();

        var sql = BuildLogicalClause(node);
        return new SqlQueryResult(sql, [.. _parameters]);
    }

    public SqlQueryResult Visit(UnaryNode node)
    {
        _paramIndex = 0;
        _parameters.Clear();

        var sql = BuildUnaryClause(node);
        return new SqlQueryResult(sql, [.. _parameters]);
    }

    private string BuildWhereClause(AstNode node)
    {
        return node switch
        {
            FilterNode filter => BuildFilterClause(filter),
            LogicalNode logical => BuildLogicalClause(logical),
            UnaryNode unary => BuildUnaryClause(unary),
            _ => node.Accept(this).Sql
        };
    }

    private string BuildLogicalClause(LogicalNode node)
    {
        var left = BuildWhereClause(node.Left);
        var op = node.Operator == LogicalOperator.And ? "AND" : "OR";
        var right = BuildWhereClause(node.Right);
        return $"({left} {op} {right})";
    }

    private string BuildUnaryClause(UnaryNode node)
    {
        var operand = BuildWhereClause(node.Operand);
        return $"NOT ({operand})";
    }

    private string BuildFilterClause(FilterNode node)
    {
        if (node.Operator is FilterOperator.In or FilterOperator.NotIn)
            return BuildInClause(node);

        var op = MapOperator(node.Operator);
        var value = node.Value is FieldReference fieldRef
            ? QuoteName(fieldRef.FieldName)
            : AddParameter(node.Value);
        return $"{QuoteName(node.ColumnName)} {op} {value}";
    }

    private string BuildInClause(FilterNode node)
    {
        var op = node.Operator == FilterOperator.In ? "IN" : "NOT IN";

        if (node.Value is not System.Collections.IEnumerable values)
            throw new InvalidOperationException($"Value for {op} filter must be a collection.");

        var sb = new StringBuilder();
        foreach (var v in values)
        {
            if (v is System.Collections.IEnumerable collection and not string)
            {
                foreach (var item in collection)
                {
                    if (sb.Length > 0) sb.Append(", ");
                    sb.Append(AddParameter(item));
                }
            }
            else
            {
                if (sb.Length > 0) sb.Append(", ");
                sb.Append(AddParameter(v));
            }
        }
        return $"{QuoteName(node.ColumnName)} {op} ({sb})";
    }

    private static string JoinColumnNames(List<string> columns)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < columns.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(QuoteName(columns[i]));
        }
        return sb.ToString();
    }

    private string JoinParameters(List<object?> values)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < values.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(AddParameter(values[i]));
        }
        return sb.ToString();
    }

    private string BuildSetClause(List<string> columns, List<object?> values)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < columns.Count; i++)
        {
            if (i > 0) sb.Append(", ");
            sb.Append(QuoteName(columns[i]));
            sb.Append(" = ");
            sb.Append(AddParameter(values[i]));
        }
        return sb.ToString();
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
        var cols = new List<string>(props.Length);
        var vals = new List<object?>(props.Length);
        foreach (var p in props)
        {
            cols.Add(p.Name);
            vals.Add(p.GetValue(payload));
        }
        return (cols, vals);
    }
}
