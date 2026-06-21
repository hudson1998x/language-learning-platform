using LLE.Auth.Features.Roles;
using LLE.Auth.Features.Users;
using LLE.Kernel.Contracts;
using LLE.Kernel.Events;
using LLE.Kernel.Registry;
using LLE.Kernel.Security;
using LLE.Sockets.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace LLE.Auth;

public class AuthModule : IModuleLoader
{
    public async Task AppStart()
    {
        FeatureLoader.LoadFeatures();

        Eventing.Eventing.Of<KestrelHttpEvents>().WebAppBuilder.Concurrent(builder =>
        {
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();
        });

        Eventing.Eventing.Of<KestrelHttpEvents>().WebApplication.Concurrent(app =>
        {
            app.UseSession();
        });

        Eventing.Eventing.Of<DatabaseEvents>().Seeding<IRoleRepository>().Concurrent(async repo =>
        {
            await repo.CreateAsync(new Role
            {
                Key = "admin",
                Name = "Admin",
                Description = "Administrator with full access"
            }, UserContext.Guest, DataOptions.Bypass);
            await repo.CreateAsync(new Role
            {
                Key = "user",
                Name = "User",
                Description = "Standard user"
            }, UserContext.Guest, DataOptions.Bypass);
            return repo;
        });
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();
    
    private Func<Task> Noop = () => Task.CompletedTask;
}