namespace LLE.Auth.Exceptions
{
    /// <summary>
    /// Thrown when a registration attempt fails, carrying the specific
    /// <see cref="FailedRegistrationCause"/> so callers can distinguish between failure reasons
    /// (e.g. for user-facing messaging).
    /// </summary>
    /// <param name="cause">The reason the registration attempt failed.</param>
    public class FailedRegistrationException(FailedRegistrationCause cause) : Exception($"Unable to register, an error occured due to {cause}")
    {
        /// <summary>The specific reason the registration attempt failed.</summary>
        public FailedRegistrationCause Cause { get; init; } = cause;
    }

    /// <summary>
    /// Enumerates the possible reasons a registration attempt can fail.
    /// </summary>
    public enum FailedRegistrationCause : byte
    {
        /// <summary>The provided email address is empty or does not pass basic format validation.</summary>
        EmailInvalid,

        /// <summary>An account with the provided email address already exists.</summary>
        EmailExists,

        /// <summary>The provided password is empty or shorter than the minimum required length.</summary>
        PasswordTooShort,

        /// <summary>The password and its confirmation (repeat) do not match.</summary>
        PasswordDoesNotMatch,
    }
}