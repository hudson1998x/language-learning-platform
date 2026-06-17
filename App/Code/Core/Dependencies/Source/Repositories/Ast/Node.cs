namespace LLE.Dependencies.Repositories.Ast;

/// <summary>
/// Represents the base type for all query AST nodes within the repository
/// execution system.
///
/// This class acts as a shared root for all query structures that can be
/// passed to an <see cref="IDatabaseAdapter"/> for execution.
/// </summary>
/// <remarks>
/// Although currently empty, this type provides a common polymorphic base
/// for future extension of the query AST, such as expressions, filters,
/// projections, joins, and composite query structures.
/// </remarks>
public class Node
{
}