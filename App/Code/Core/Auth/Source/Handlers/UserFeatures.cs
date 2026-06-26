using LLE.Auth.Dto;
using LLE.Auth.Exceptions;
using LLE.Auth.Features.Roles;
using LLE.Auth.Features.Users;
using LLE.Auth.Utilities;
using LLE.Kernel.Registry;
using LLE.Kernel.Security;
using Microsoft.AspNetCore.Http;

namespace LLE.Auth.Handlers;

public static class UserFeatures
{
    public static async ValueTask<LoginResponse> Login(LoginBody body, HttpContext context)
    {
        var userService = ServiceCatalog.GetService<UserService>();

        var user = await userService.Login(context, body);

        if (user is null)
        {
            return new LoginResponse()
            {
                Success = false,
                Message = "Something unexpected happened during the login process"
            };
        }

        return new LoginResponse()
        {
            Success = true,
            Message = "Login Successful",
            User = user
        };
    }

    public static async ValueTask<UserAuthStateResponse> UserAuthState(object body, HttpContext context)
    {
        var userService = ServiceCatalog.GetService<UserService>();
        var roleRepository = RepositoryCatalog.GetRepository<IRoleRepository>();
        
        var currentUser = await userService.GetCurrentUser(context);

        Role? role = null;

        if (currentUser?.RoleId is not null)
        {
            role = await roleRepository.FindByIdAsync(currentUser.RoleId.Value, UserContext.FromHttpContext(context), DataOptions.Bypass);
        }

        role ??= await roleRepository.FindByKeyAsync("guest", UserContext.Guest, DataOptions.Bypass);
        
        return new()
        {
            Success = true,
            Message = "Endpoint okay",
            User = currentUser,
            Role = role
        };
    }

    public static async ValueTask<RegisterResponse> Register(RegisterBody body, HttpContext context)
    {
        var userService = ServiceCatalog.GetService<UserService>();

        if (body.Password != body.ConfirmPassword)
        {
            throw new RegistrationException("The passwords do not match");
        }

        var user = new User()
        {
            Email = body.Email,
            Password = PasswordHasher.Hash(body.Password),
        };

        user = await userService.RegisterUser(context, user);

        return new RegisterResponse()
        {
            Success = true,
            Message = "Endpoint okay",
            User = user
        };
    }
}