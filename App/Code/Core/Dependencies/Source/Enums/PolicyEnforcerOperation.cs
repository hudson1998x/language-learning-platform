using LLE.Dependencies.Repositories;

namespace LLE.Dependencies.Enums;

/// <summary>
/// Represents a repository operation that may be evaluated by an
/// <see cref="IPolicyEnforcer"/> before execution.
/// </summary>
public enum PolicyEnforcerOperation : byte
{
    /// <summary>
    /// Retrieve a single entity or resource.
    /// </summary>
    Load,

    /// <summary>
    /// Create a new entity or resource.
    /// </summary>
    Create,

    /// <summary>
    /// Modify an existing entity or resource.
    /// </summary>
    Update,

    /// <summary>
    /// Remove an existing entity or resource.
    /// </summary>
    Delete,

    /// <summary>
    /// Retrieve multiple entities or resources.
    /// </summary>
    List
}