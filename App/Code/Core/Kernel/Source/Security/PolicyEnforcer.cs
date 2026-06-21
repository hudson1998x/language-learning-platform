using System.Reflection;
using LLE.Kernel.DataQL.Ast;

namespace LLE.Kernel.Security;

public static class PolicyEnforcer
{
    private static readonly Dictionary<(Guid? roleId, string operation), PermissionLevel> _permissions = new();
    private static bool _sealed;
    private static readonly object _lock = new();

    public static void SetRule(Guid? roleId, string operation, PermissionLevel level)
    {
        lock (_lock)
        {
            if (_sealed)
                throw new InvalidOperationException("PolicyEnforcer is sealed and cannot be modified.");

            _permissions[(roleId, operation)] = level;
        }
    }

    public static void Seal()
    {
        lock (_lock)
        {
            _sealed = true;
        }
    }

    public static PermissionLevel GetPermission(UserContext context, string operation)
    {
        if (context.RoleId.HasValue &&
            _permissions.TryGetValue((context.RoleId.Value, operation), out var level))
            return level;

        if (_permissions.TryGetValue((null, operation), out var guestLevel))
            return guestLevel;

        return PermissionLevel.NotAllowed;
    }

    public static void Enforce(AstNode node, UserContext context, DataOptions options)
    {
        if (options == DataOptions.Bypass)
            return;

        var entityType = node.EntityType;
        var action = node switch
        {
            WriteQueryNode w when w.Where is null => "create",
            WriteQueryNode => "update",
            DeleteQueryNode => "delete",
            ReadQueryNode => "read",
            _ => "unknown"
        };

        var operation = $"{entityType?.Name ?? "Unknown"}_{action}";
        var permission = GetPermission(context, operation);

        if (permission == PermissionLevel.NotAllowed)
            throw new PermissionException($"Permission denied for operation '{operation}'.");

        if (permission == PermissionLevel.OwnedOnly)
            ApplyOwnershipFilter(node, context);
    }

    private static void ApplyOwnershipFilter(AstNode node, UserContext context)
    {
        if (!context.UserId.HasValue)
            throw new PermissionException("Authentication required for this operation.");

        var userId = context.UserId.Value;
        var entityType = node.EntityType;

        if (entityType is null)
            return;

        var userIdProp = entityType.GetProperty("UserId", BindingFlags.Public | BindingFlags.Instance);
        var idProp = entityType.GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);

        string columnName;
        if (userIdProp is not null)
            columnName = "UserId";
        else if (idProp is not null && entityType.Name == "User")
            columnName = "Id";
        else
            return;

        var filter = new FilterNode
        {
            ColumnName = columnName,
            Operator = FilterOperator.Equals,
            Value = userId
        };

        switch (node)
        {
            case ReadQueryNode read:
                read.Where = read.Where is null
                    ? filter
                    : new LogicalNode
                    {
                        Operator = LogicalOperator.And,
                        Left = read.Where,
                        Right = filter
                    };
                break;

            case WriteQueryNode write:
                if (write.Where is null)
                {
                    if (userIdProp is not null)
                        userIdProp.SetValue(write.Payload, userId);
                }
                else
                {
                    write.Where = new LogicalNode
                    {
                        Operator = LogicalOperator.And,
                        Left = write.Where,
                        Right = filter
                    };
                }
                break;

            case DeleteQueryNode delete:
                delete.Where = delete.Where is null
                    ? filter
                    : new LogicalNode
                    {
                        Operator = LogicalOperator.And,
                        Left = delete.Where,
                        Right = filter
                    };
                break;
        }
    }
}
