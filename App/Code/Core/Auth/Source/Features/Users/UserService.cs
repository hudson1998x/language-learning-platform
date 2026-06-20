using LLE.Auth.Dto;
using LLE.Auth.Exceptions;
using LLE.Auth.Utilities;
using LLE.Kernel.Attributes;
using Microsoft.AspNetCore.Http;

namespace LLE.Auth.Features.Users;

[Service]
public class UserService(IUserRepository userRepository)
{
    public async Task<User?> Login(HttpContext context, LoginBody body)
    {
        if (context.Session.GetString("UserId") != null)
        {
            throw new LoginException("User is already logged in.");
        }
        
        var user = await userRepository.GetByEmailAsync(body.Email);

        if (user is null)
        {
            throw new LoginException("Unable to find user with the supplied email.");
        }

        if (!PasswordHasher.Verify(body.Password, user.Password))
        {
            throw new LoginException("Unable to find user with the supplied email, or password combination.");
        }
        
        context.Session.SetString("UserId", user.Id.ToString());

        return user;
    }
}