using LLE.Dependencies.Enums;
using LLE.Dependencies.Request;

namespace LLE.Dependencies.Repositories;

/// <summary>
/// Defines a policy enforcement component responsible for determining whether
/// a particular operation may be performed against a target object.
///
/// Policy enforcers provide a centralised mechanism for implementing
/// authorisation, ownership checks, role-based access control, and other
/// application-specific security rules independently of repository and
/// database implementations.
/// </summary>
public interface IPolicyEnforcer
{
    /// <summary>
    /// Evaluates whether the specified operation is permitted for the supplied
    /// target object.
    /// </summary>
    /// <param name="type">
    /// The entity or resource type being accessed.
    /// </param>
    /// <param name="target">
    /// The target object involved in the operation.
    /// </param>
    /// <param name="context">
    /// Information about the current user
    /// </param>
    /// <param name="operation">
    /// The operation being requested.
    /// </param>
    /// <returns>
    /// <c>true</c> if the operation is permitted; otherwise <c>false</c>.
    /// </returns>
    public Task<bool> CanPerformOperation(
        Type type,
        object target,
        SessionContext context,
        PolicyEnforcerOperation operation
    );
}