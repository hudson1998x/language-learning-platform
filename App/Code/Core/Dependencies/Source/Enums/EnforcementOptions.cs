namespace LLE.Dependencies.Enums;

/// <summary>
/// Specifies how policy enforcement should behave when evaluating
/// whether an operation is permitted.
///
/// These options allow repository or execution layers to control
/// whether security checks are strictly enforced or bypassed in
/// controlled scenarios (for example, internal system operations
/// or trusted execution contexts).
/// </summary>
public enum EnforcementOptions : byte
{
    /// <summary>
    /// Standard enforcement behaviour.
    ///
    /// All configured policies are evaluated and must pass before
    /// an operation is permitted.
    /// </summary>
    Default,

    /// <summary>
    /// Bypasses permission checks during policy evaluation.
    ///
    /// Intended for trusted internal flows where enforcement is
    /// explicitly unnecessary or would cause circular dependency
    /// issues in system-level operations.
    /// </summary>
    IgnorePermissions
}