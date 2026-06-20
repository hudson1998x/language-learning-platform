namespace LLE.Auth.Dto;

public class RegisterBody
{
    public string Email { get; set; }
    
    public string Password { get; set; }
    
    public string ConfirmPassword { get; set; }
}