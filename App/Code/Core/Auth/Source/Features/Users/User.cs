using LLE.Kernel.Attributes;
using LLE.Kernel.Data;

namespace LLE.Auth.Features.Users;

[Entity]
public class User : ContentWithId
{
    public required string Email { get; set; }
    
    public required string Password { get; set; }
}