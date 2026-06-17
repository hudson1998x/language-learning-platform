using LLE.Dependencies.Enums;

namespace LLE.Dependencies.Repositories.Ast;

/// <summary>
/// Represents a database query expressed as an abstract syntax tree (AST) node.
///
/// This is the root structure passed from the repository layer into the
/// database adapter for execution. It defines the target table and the
/// high-level operation to perform.
/// </summary>
/// <remarks>
/// Query nodes are constructed and validated at the repository layer.
/// Database adapters must treat this as an already-validated execution
/// instruction and should not perform additional policy or structural checks.
/// </remarks>
public class QueryNode : Node
{
    /// <summary>
    /// The name of the database table targeted by this query.
    /// </summary>
    public required string TableName { get; init; }

    /// <summary>
    /// The type of operation to perform against the target table,
    /// such as create, update, delete, or read operations.
    /// </summary>
    public required OperationType OperationType { get; init; }
}