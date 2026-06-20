using LLE.Auth.Dto;
using LLE.Auth.Exceptions;
using LLE.Auth.Features.Roles;
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

    public async Task<User> RegisterUser(HttpContext context, User user)
    {
        
        if (await AccountExistsWithEmail(user.Email))
        {
            throw new RegistrationException("An account already exists with this email");
        }
        
        user = await userRepository.CreateAsync(user);
        context.Session.SetString("UserId", user.Id.ToString());
        return user;
    }

    public async Task<bool> AccountExistsWithEmail(string email)
    {
        var user = await userRepository.GetByEmailAsync(email);
        
        return user != null;
    }

    public async Task<User?> GetCurrentUser(HttpContext context)
    {
        if (!context.Session.IsAvailable)
        {
            return null;
        }
        
        var userIdStr = context.Session.GetString("UserId");

        if (string.IsNullOrEmpty(userIdStr))
        {
            return null;
        }

        var userId = Guid.Parse(userIdStr);
        
        return await userRepository.FindByIdAsync(userId);
    }
}