using LLE.Auth.Features.Roles;
using LLE.Auth.Features.Users;

namespace LLE.Auth.Dto;

public class UserAuthStateResponse : CommonApiResponse
{
    public User? User { get; set; }
    
    public Role? Role { get; set; }
}