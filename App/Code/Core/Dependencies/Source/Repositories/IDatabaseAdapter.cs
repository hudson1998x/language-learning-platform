using LLE.Dependencies.Repositories.Ast;

namespace LLE.Dependencies.Repositories;

/// <summary>
/// Represents a low-level execution engine responsible for evaluating
/// repository-generated query AST nodes against a backing data store.
///
/// The adapter does not perform policy checks, query construction, or
/// type mapping decisions. It operates purely as an execution boundary,
/// taking a fully-formed <see cref="QueryNode"/> and returning a
/// materialised result.
/// </summary>
public interface IDatabaseAdapter
{
    /// <summary>
    /// Establishes a connection to the underlying data store.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous connection operation.
    /// </returns>
    public Task ConnectAsync();

    /// <summary>
    /// Closes the active connection to the underlying data store and releases
    /// any associated resources.
    /// </summary>
    /// <returns>
    /// A task representing the asynchronous disconnection operation.
    /// </returns>
    public Task DisconnectAsync();

    /// <summary>
    /// Executes a pre-built query AST node against the underlying data store
    /// and materialises the result into the requested type.
    ///
    /// The adapter assumes the provided node is already validated, authorised,
    /// and correctly shaped by higher-level repository logic.
    /// </summary>
    /// <typeparam name="T">
    /// The type into which the result of the query should be materialised.
    /// </typeparam>
    /// <param name="node">
    /// The query AST representing the operation to execute.
    /// </param>
    /// <param name="cancellationToken">
    /// Propagates notification that the execution should be cancelled.
    /// </param>
    /// <returns>
    /// The materialised result of executing the query.
    /// </returns>
    public Task<T> ExecuteAsync<T>(QueryNode node, CancellationToken cancellationToken);
}