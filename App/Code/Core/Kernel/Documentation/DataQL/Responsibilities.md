# DataQL — Responsibilities

DataQL is a lightweight, embeddable query language for the LLE Kernel. It provides a string-based DSL for expressing data queries (filtering, sorting, pagination) and mutations (create/update/delete) against entities, similar in concept to OData or GraphQL but custom-built and tightly scoped.

## Pipeline

Processing follows three phases:

### 1. Tokenisation (`Tokeniser/`)

`TokenArrayBuilder.Parse(source)` converts a raw query string into a flat `List<Token>`. Recognised token types:

- Identifiers (field names, keywords)
- Named parameters (`:paramName`)
- String literals (`"value"`)
- Numeric literals (`123`, `3.14`)
- Comparison operators (`=`, `!=`, `>`, `<`, `>=`, `<=`)
- Keywords: `and`, `or`, `not`, `in`, `like`
- Grouping/punctuation: `(`, `)`, `,`, `.`

### 2. Parsing (`Ast/`)

`AstParser.Parse(entityType, query, parameters?)` consumes the token stream via a recursive-descent parser and produces an **Abstract Syntax Tree (AST)**. Operator precedence (lowest to highest):

```
or  →  and  →  not (prefix)  →  primary (grouping)  →  comparison
```

The result is always a `ReadQueryNode` wrapping the parsed filter expression.

### 3. Traversal (`Ast/IAstVisitor.cs`)

The AST uses the **Visitor pattern** — each node implements `Accept<TResult>(IAstVisitor<TResult>)`, and the visitor interface has one `Visit` overload per concrete node type. This decouples tree structure from downstream processing (e.g., SQL generation, in-memory filtering, validation).

## AST Node Types

| Node | Purpose |
|---|---|
| `ReadQueryNode` | Read/select query: table name, optional `Where`, `OrderBy`, `Pagination`, `IsCount` flag |
| `WriteQueryNode` | Insert/update: table name, payload, optional `Where` |
| `DeleteQueryNode` | Delete: table name, payload, optional `Where` |
| `FilterNode` | Single predicate: column name, operator, value |
| `LogicalNode` | Binary `AND` / `OR` combining two sub-expressions |
| `UnaryNode` | Unary `NOT` applied to a single operand |
| `SortOption` | Sort field + direction |
| `Pagination` | Page number + limit |

## Declarative Binding

The `[Query]` attribute (`Attributes/QueryAttribute.cs`) decorates C# methods with a DataQL query string, enabling the framework to reflect on the method at runtime, parse the query, and execute it.

## Enums

- `OperationKind` — `Read`, `Write`, `Delete` (classifies the AST node's operation).
- `FilterOperator` — `Equals`, `NotEquals`, `LessThan`, `LessThanOrEquals`, `GreaterThan`, `GreaterThanOrEquals`, `In`, `NotIn`, `Like`, `NotLike`.
- `LogicalOperator` — `And`, `Or`.
- `UnaryOperator` — `Not`.
