namespace LLE.Auth.Exceptions;

/// <summary>
/// Thrown when a login attempt fails, carrying the specific <see cref="FailCause"/>
/// so callers can distinguish between failure reasons (e.g. for user-facing messaging).
/// </summary>
/// <param name="cause">The reason the login attempt failed.</param>
public class FailedAuthenticationException(FailCause cause) : Exception("Failed to login, reason " + cause)
{
    /// <summary>The specific reason the login attempt failed.</summary>
    public readonly FailCause FailCause = cause;
}

/// <summary>
/// Enumerates the possible reasons a login attempt can fail.
/// </summary>
public enum FailCause
{
    /// <summary>No email address was provided.</summary>
    EmptyEmail,

    /// <summary>No user exists with the provided email address.</summary>
    InvalidEmail,

    /// <summary>No password was provided.</summary>
    EmptyPassword,

    /// <summary>The provided password does not match the account's password.</summary>
    IncorrectPassword
}