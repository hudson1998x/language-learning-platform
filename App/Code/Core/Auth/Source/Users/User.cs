using LLE.Kernel.Attributes;
using LLE.Kernel.Data;

namespace LLE.Auth.Users;

/// <summary>
/// Represents a registered user account.
/// </summary>
[Entity]
public class User : ContentWithId
{
    /// <summary>The user's display name.</summary>
    public string Name { get; set; }

    /// <summary>The user's email address, used as their login identifier.</summary>
    public string Email { get; set; }

    /// <summary>The user's hashed password. Never stores the plaintext password.</summary>
    public string Password { get; set; }

    /// <summary>The id of the role assigned to this user, used for authorization.</summary>
    public Guid RoleId { get; set; }
}