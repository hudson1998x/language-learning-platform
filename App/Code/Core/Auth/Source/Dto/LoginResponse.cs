using LLE.Auth.Features.Users;

namespace LLE.Auth.Dto;

public class LoginResponse : CommonApiResponse
{
    public User? User { get; set; } = null;
}