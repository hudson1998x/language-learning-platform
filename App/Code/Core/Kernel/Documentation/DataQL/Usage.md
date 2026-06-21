# DataQL — Usage

## Writing DataQL Queries

DataQL queries are plain strings with a SQL-like filter syntax. Examples:

```
Status = :status and Age > 21
Email = "admin@example.com"
Name like :searchPattern
CategoryId in :categoryIds
not (Status = 0)
User.Address.City = :city
```

### Supported Operators

| Operator | Meaning |
|---|---|
| `=` | Equals |
| `!=` | Not equals |
| `>` | Greater than |
| `<` | Less than |
| `>=` | Greater than or equal |
| `<=` | Less than or equal |
| `in` | Value in a list |
| `not in` | Value not in a list |
| `like` | Pattern match |
| `not like` | Pattern non-match |

### Logical Operators

- `and` — both conditions must be true
- `or` — either condition may be true
- `not` — negates the following expression

### Values

- **Named parameters**: `:paramName` (recommended for safe parameterized queries)
- **String literals**: `"hello world"`
- **Numeric literals**: `42`, `3.14`
- **Boolean literals**: `true`, `false`
- **Null literal**: `null`
- **Field paths**: Dot notation for nested fields, e.g., `Address.City`

## Binding Queries to Methods

Use the `[Query]` attribute on repository interface methods:

```csharp
using LLE.Kernel.DataQL.Attributes;

public interface IUserRepository : IEntityRepository<User>
{
    [Query("Email = :email")]
    Task<User?> FindByEmailAsync(string email, UserContext context, DataOptions options);

    [Query("Status in :statuses and CreatedAt >= :since")]
    Task<List<User>> FindRecentByStatusAsync(List<int> statuses, DateTime since, UserContext context, DataOptions options);
}
```

Parameter names in the query (`:email`, `:statuses`, `:since`) are matched **by name** to the method's parameters.

## Parsing Queries Programmatically

```csharp
using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.DataQL.Tokeniser;

// Parse with parameters
var parameters = new Dictionary<string, object?>
{
    ["status"] = 1,
    ["minAge"] = 21
};

AstNode ast = AstParser.Parse(typeof(User), "Status = :status and Age > :minAge", parameters);
// Result: ReadQueryNode with TableName = "User", Where = LogicalNode(And, FilterNode(Status, =), FilterNode(Age, >))
```

The resulting `ReadQueryNode` can be traversed by implementing `IAstVisitor<TResult>`:

```csharp
public class SqlGenerator : IAstVisitor<string>
{
    public string Visit(FilterNode node) { /* emit column = value */ }
    public string Visit(LogicalNode node) { /* emit left AND/OR right */ }
    public string Visit(UnaryNode node) { /* emit NOT (operand) */ }
    // ... other Visit methods
}
```

## Best Practices

- **Always use named parameters** (`:paramName`) instead of inline values to avoid injection risks and enable query caching.
- **Parameter names are case-sensitive** and must match the method parameter names exactly.
- **Dot notation** enables filtering on nested entity properties where the data layer supports it.
- Sorting and pagination are set on the `ReadQueryNode` after parsing by the consuming infrastructure (e.g., the repository proxy builder).
