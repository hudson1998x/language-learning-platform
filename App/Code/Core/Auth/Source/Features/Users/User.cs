using LLE.Kernel.Attributes;
using LLE.Kernel.Data;

namespace LLE.Auth.Features.Users;

[Entity]
public class User : ContentWithId
{
    public string Email { get; set; }
    
    public string Password { get; set; }
}