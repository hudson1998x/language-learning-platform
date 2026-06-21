using System.Reflection;
using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.Registry;
using LLE.SQLiteAdapter.Configurations;
using Microsoft.Data.Sqlite;

namespace LLE.SQLiteAdapter;

public class SQLiteAdapter : IDatabaseAdapter
{
    private readonly HashSet<Type> _initializedTables = [];

    public void EnsureTable(Type entityType)
    {
        if (!_initializedTables.Add(entityType))
            return;

        var tableName = entityType.Name;

        using var conn = GetConnection();
        conn.Open();

        // Step 1: create the table with the full current schema (no-op if already exists)
        var columns = new List<string> { "[Id] TEXT NOT NULL PRIMARY KEY" };

        foreach (var prop in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.Name == "Id")
                continue;

            var (colType, notNull) = ResolveColumnType(prop.PropertyType);
            columns.Add($"[{prop.Name}] {colType}{(notNull ? " NOT NULL" : " NULL")}");
        }

        using (var cmd = new SqliteCommand(
            $"CREATE TABLE IF NOT EXISTS [{tableName}] (\n  {string.Join(",\n  ", columns)}\n);", conn))
        {
            cmd.ExecuteNonQuery();
        }

        // Step 2: sync — add any columns that exist on the entity but not yet in the table.
        // This handles the case where the table was created by an earlier version of the entity class.
        var existingColumns = GetExistingColumnNames(conn, tableName);

        foreach (var prop in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (existingColumns.Contains(prop.Name))
                continue;

            var (colType, notNull) = ResolveColumnType(prop.PropertyType);
            var colDef = $"[{prop.Name}] {colType}";

            if (notNull)
            {
                colDef += " NOT NULL";
                colDef += $" DEFAULT {GetDefaultValue(prop.PropertyType)}";
            }
            else
            {
                colDef += " NULL";
            }

            using var alterCmd = new SqliteCommand(
                $"ALTER TABLE [{tableName}] ADD COLUMN {colDef};", conn);
            alterCmd.ExecuteNonQuery();
        }

        // Step 3: create unique indexes for [Unique] properties
        foreach (var prop in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop.GetCustomAttribute<UniqueAttribute>() is null)
                continue;

            var indexName = $"IX_{tableName}_{prop.Name}";
            using var idxCmd = new SqliteCommand(
                $"CREATE UNIQUE INDEX IF NOT EXISTS [{indexName}] ON [{tableName}]([{prop.Name}]);", conn);
            idxCmd.ExecuteNonQuery();
        }
    }

    private static HashSet<string> GetExistingColumnNames(SqliteConnection conn, string tableName)
    {
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using var cmd = new SqliteCommand($"PRAGMA table_info([{tableName}]);", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
            columns.Add(reader.GetString(1));
        return columns;
    }

    private static string GetDefaultValue(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null)
            return "NULL";

        if (type.IsEnum)
            return "0";

        return Type.GetTypeCode(type) switch
        {
            TypeCode.Boolean => "0",
            TypeCode.Byte or TypeCode.Int16 or TypeCode.UInt16
                or TypeCode.Int32 or TypeCode.UInt32
                or TypeCode.Int64 or TypeCode.UInt64 or TypeCode.SByte => "0",
            TypeCode.Single or TypeCode.Double or TypeCode.Decimal => "0",
            TypeCode.String or TypeCode.Char => "''",
            TypeCode.DateTime => "'0001-01-01T00:00:00'",
            _ => type == typeof(Guid) ? $"'{Guid.Empty}'" : "''"
        };
    }

    public async Task<object> ExecuteQuery(AstNode node)
    {
        var builder = new SqliteQueryBuilder();
        var result = node.Accept(builder);

        await using var conn = GetConnection();
        await conn.OpenAsync();

        await using var cmd = new SqliteCommand(result.Sql, conn);
        foreach (var param in result.Parameters)
            _ = cmd.Parameters.Add(param);

        if (node is ReadQueryNode readNode)
        {
            var entityType = readNode.EntityType
                ?? throw new InvalidOperationException(
                    "ReadQueryNode must have an EntityType set to map results.");

            var rows = new List<Dictionary<string, object?>>();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var row = new Dictionary<string, object?>();
                for (var i = 0; i < reader.FieldCount; i++)
                    row[reader.GetName(i)] = reader.IsDBNull(i) ? null : reader.GetValue(i);
                rows.Add(row);
            }

            var entities = rows.Select(r => MapToEntity(r, entityType)).ToList();

            return entities;
        }

        if (node is WriteQueryNode writeNode)
        {
            await cmd.ExecuteNonQueryAsync();
            return writeNode.Payload;
        }

        if (node is DeleteQueryNode deleteNode)
        {
            await cmd.ExecuteNonQueryAsync();
            return deleteNode.Payload;
        }

        return await cmd.ExecuteNonQueryAsync();
    }

    private static object MapToEntity(Dictionary<string, object?> row, Type entityType)
    {
        var instance = Activator.CreateInstance(entityType)!;

        foreach (var prop in entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!row.TryGetValue(prop.Name, out var value) || value is null)
                continue;

            if (value.GetType() == prop.PropertyType)
            {
                prop.SetValue(instance, value);
                continue;
            }

            prop.SetValue(instance, ConvertValue(value, prop.PropertyType));
        }

        return instance;
    }

    private static object? ConvertValue(object value, Type targetType)
    {
        var underlying = Nullable.GetUnderlyingType(targetType);
        if (underlying is not null)
            targetType = underlying;

        if (targetType.IsEnum)
            return Enum.ToObject(targetType, Convert.ChangeType(value, Enum.GetUnderlyingType(targetType)));

        if (targetType == typeof(Guid))
            return value is string s ? Guid.Parse(s) : value;

        if (targetType == typeof(DateTime))
            return value is string dt ? DateTime.Parse(dt, null, System.Globalization.DateTimeStyles.RoundtripKind) : value;

        if (targetType == typeof(DateTimeOffset))
            return value is string dto ? DateTimeOffset.Parse(dto) : value;

        if (targetType == typeof(bool) && value is long l)
            return l != 0;

        if (value is long big && targetType == typeof(int))
            return (int)big;

        if (value is long big2 && targetType == typeof(short))
            return (short)big2;

        return Convert.ChangeType(value, targetType);
    }

    private SqliteConnection GetConnection()
    {
        var dbPath = GetAbsoluteDbPath();

        var dir = Path.GetDirectoryName(dbPath);
        if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        return new SqliteConnection($"Data Source={dbPath}");
    }

    private static string GetAbsoluteDbPath()
    {
        var config = ConfigurationCatalog.GetConfiguration<SqliteConfiguration>();

        if (Path.IsPathRooted(config.AppDbFile))
            return config.AppDbFile;

        return Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, config.AppDbFile));
    }

    private static (string typeName, bool notNull) ResolveColumnType(Type type)
    {
        var underlying = Nullable.GetUnderlyingType(type);
        if (underlying is not null)
        {
            var (baseType, _) = ResolveColumnType(underlying);
            return (baseType, false);
        }

        if (type.IsEnum)
            return ("INTEGER", true);

        var notNull = type.IsValueType;
        var typeName = Type.GetTypeCode(type) switch
        {
            TypeCode.Boolean => "INTEGER",
            TypeCode.Byte or TypeCode.Int16 or TypeCode.UInt16
                or TypeCode.Int32 or TypeCode.UInt32
                or TypeCode.Int64 or TypeCode.UInt64
                or TypeCode.SByte => "INTEGER",
            TypeCode.Single or TypeCode.Double or TypeCode.Decimal => "REAL",
            TypeCode.String or TypeCode.Char => "TEXT",
            TypeCode.DateTime => "TEXT",
            _ => type == typeof(Guid) || type == typeof(byte[]) ? "TEXT" : "TEXT"
        };

        return (typeName, notNull);
    }
}
