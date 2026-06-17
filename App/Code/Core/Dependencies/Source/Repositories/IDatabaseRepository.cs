using LLE.Dependencies.Enums;
using LLE.Dependencies.Request;

namespace LLE.Dependencies.Repositories;

/// <summary>
/// Defines the common persistence operations available to a repository
/// backed by a database adapter.
///
/// Repository implementations act as a bridge between repository contracts,
/// database adapters, and policy enforcement components, allowing data access
/// and security concerns to remain decoupled from application logic.
/// </summary>
/// <typeparam name="T">
/// The entity type managed by the repository.
/// </typeparam>
public interface IDatabaseRepository<T> where T : class
{
    /// <summary>
    /// Assigns the database adapter responsible for executing persistence
    /// operations for this repository.
    /// </summary>
    /// <param name="adapter">
    /// The database adapter used to communicate with the underlying data store.
    /// </param>
    public void SetDatabaseEngine(IDatabaseAdapter adapter);

    /// <summary>
    /// Assigns the policy enforcer responsible for validating and authorising
    /// repository operations.
    /// </summary>
    /// <param name="policyEnforcer">
    /// The policy enforcement component used to evaluate repository access rules.
    /// </param>
    public void SetPolicyEnforcer(IPolicyEnforcer policyEnforcer);

    /// <summary>
    /// Persists a new entity to the underlying data store.
    /// </summary>
    /// <param name="entity">
    /// The entity to create.
    /// </param>
    /// <param name="context">
    /// Information about the current user
    /// </param>
    /// <param name="options">
    /// Specifies how policy enforcement should behave when evaluating
    /// whether an operation is permitted.
    /// </param>
    /// <returns>
    /// The created entity, potentially updated with database-generated values.
    /// </returns>
    public Task<T> CreateAsync(T entity, SessionContext context, EnforcementOptions options = EnforcementOptions.Default);

    /// <summary>
    /// Updates an existing entity identified by the specified identifier.
    /// </summary>
    /// <param name="entity">
    /// The updated entity data.
    /// </param>
    /// <param name="id">
    /// The identifier of the entity to update.
    /// </param>
    /// <param name="context">
    /// Information about the current user
    /// </param>
    /// <param name="options">
    /// Specifies how policy enforcement should behave when evaluating
    /// whether an operation is permitted.
    /// </param>
    /// <returns>
    /// The updated entity.
    /// </returns>
    public Task<T> UpdateAsync(T entity, Guid id, SessionContext context, EnforcementOptions options = EnforcementOptions.Default);

    /// <summary>
    /// Creates the entity if it does not exist, or updates the existing entity
    /// if a corresponding record is already present.
    /// </summary>
    /// <param name="entity">
    /// The entity to create or update.
    /// </param>
    /// <param name="context">
    /// Information about the current user
    /// </param>
    /// <param name="options">
    /// Specifies how policy enforcement should behave when evaluating
    /// whether an operation is permitted.
    /// </param>
    /// <returns>
    /// The persisted entity.
    /// </returns>
    public Task<T> CreateOrUpdateAsync(T entity, SessionContext context, EnforcementOptions options = EnforcementOptions.Default);

    /// <summary>
    /// Removes the specified entity from the data store.
    /// </summary>
    /// <param name="entity">
    /// The entity to delete.
    /// </param>
    /// <param name="context">
    /// Information about the current user
    /// </param>
    /// <param name="options">
    /// Specifies how policy enforcement should behave when evaluating
    /// whether an operation is permitted.
    /// </param>
    /// <returns>
    /// The deleted entity.
    /// </returns>
    public Task<T> DeleteAsync(T entity, SessionContext context, EnforcementOptions options = EnforcementOptions.Default);

    /// <summary>
    /// Removes an entity identified by the specified identifier.
    /// </summary>
    /// <param name="id">
    /// The identifier of the entity to delete.
    /// </param>
    /// <param name="context">
    /// Information about the current user
    /// </param>
    /// <param name="options">
    /// Specifies how policy enforcement should behave when evaluating
    /// whether an operation is permitted.
    /// </param>
    /// <returns>
    /// The deleted entity.
    /// </returns>
    public Task<T> DeleteAsync(Guid id, SessionContext context, EnforcementOptions options = EnforcementOptions.Default);

    /// <summary>
    /// Retrieves an entity by its unique identifier.
    /// </summary>
    /// <param name="guid">
    /// The identifier of the entity to retrieve.
    /// </param>
    /// <param name="context">
    /// Information about the current user
    /// </param>
    /// <param name="options">
    /// Specifies how policy enforcement should behave when evaluating
    /// whether an operation is permitted.
    /// </param>
    /// <returns>
    /// The matching entity if found.
    /// </returns>
    public Task<T> FindByIdAsync(Guid guid, SessionContext context, EnforcementOptions options = EnforcementOptions.Default);
}