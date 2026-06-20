using LLE.Auth.Dto;
using LLE.Auth.Features.Users;
using LLE.Kernel.Registry;
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
}