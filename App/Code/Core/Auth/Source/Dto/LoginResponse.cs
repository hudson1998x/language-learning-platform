using LLE.Auth.Features.Users;

namespace LLE.Auth.Dto;

public class LoginResponse
{
    public bool Success { get; set; }
    
    public string Message { get; set; } = string.Empty;

    public User? User { get; set; } = null;
}