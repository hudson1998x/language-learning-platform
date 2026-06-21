using LLE.Kernel.Attributes;
using LLE.Kernel.Data;

namespace LLE.Auth.Features.Users;

[Entity]
public class User : ContentWithId
{
    [Unique]
    public required string Email { get; set; }
    
    public required string Password { get; set; }

    public string FullName { get; set; } = string.Empty;
    
    public Guid? RoleId { get; set; }
}