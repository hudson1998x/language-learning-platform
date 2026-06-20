namespace LLE.Auth.Dto;

/// <summary>
/// Represents the credentials submitted when a user attempts to log in.
/// </summary>
public class LoginPayload
{
    /// <summary>The email address of the account to log in to.</summary>
    public string Email { get; init; }

    /// <summary>The password for the account.</summary>
    public string Password { get; init; }
}