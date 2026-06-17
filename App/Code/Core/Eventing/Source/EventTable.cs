namespace LLE.Eventing;

/// <summary>
/// Base class for event tables. Derive from this class to group related events together —
/// for example, <c>UserEvents : EventTable</c> exposing members like <c>Created</c> or <c>Deleted</c>.
/// Acts purely as a marker / type-restricting base for use with <see cref="Eventing.Of{T}"/>.
/// </summary>
/// <remarks>
/// Each derived <see cref="EventTable"/> subtype must have an accessible parameterless
/// constructor, since <see cref="Eventing"/> creates exactly one singleton instance per
/// subtype via reflection the first time it is requested.
/// </remarks>
public class EventTable
{
    
}