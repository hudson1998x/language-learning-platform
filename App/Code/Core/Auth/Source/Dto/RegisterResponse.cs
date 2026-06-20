using LLE.Auth.Features.Users;

namespace LLE.Auth.Dto;

public class RegisterResponse : CommonApiResponse
{
    public User User { get; set; }
}