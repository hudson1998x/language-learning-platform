using LLE.Auth.Configurations;
using LLE.Auth.Events;
using LLE.Auth.Features.Roles;
using LLE.Auth.Features.Users;
using LLE.Auth.Utilities;
using LLE.Kernel.Contracts;
using LLE.Kernel.Events;
using LLE.Kernel.Registry;
using LLE.Kernel.Security;
using LLE.Sockets.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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

        Eventing.Eventing.Of<KestrelHttpEvents>().WebApplication.Concurrent(app =>
        {
            app.Use(async (context, next) =>
            {
                var config = ConfigurationCatalog.GetConfiguration<SingleUserConfiguration>();

                if (config.IsSingleUserOnly && context.Session.GetString("UserId") == null)
                {
                    var userRepository = RepositoryCatalog.GetRepository<IUserRepository>();
                    var users = await userRepository.FindAllAsync(UserContext.Guest, DataOptions.Bypass);
                    var user = users.FirstOrDefault();

                    if (user is not null)
                    {
                        context.Session.SetString("UserId", user.Id.ToString());
                        if (user.RoleId.HasValue)
                            context.Session.SetString("RoleId", user.RoleId.Value.ToString());
                    }
                }

                await next();
            });
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
            await repo.CreateAsync(new Role
            {
                Key = "guest",
                Name = "Guest",
                Description = "Not logged in yet."
            }, UserContext.Guest, DataOptions.Bypass);
            
            await Eventing.Eventing.Of<RolesEventTable>().Setup.DispatchAsync(repo);
            await Eventing.Eventing.Of<RolesEventTable>().Ready.DispatchAsync(repo);
            
            return repo;
        });

        Eventing.Eventing.Of<RolesEventTable>().Ready.Concurrent(async roleRepository =>
        {
            var adminRole = await roleRepository.FindByKeyAsync("admin", UserContext.Guest, DataOptions.Bypass);
            var userRepository = RepositoryCatalog.GetRepository<IUserRepository>();

            if (await userRepository.TotalRecords(UserContext.Guest, DataOptions.Bypass) == 0)
            {
                var user = new User()
                {
                    RoleId = adminRole.Id,
                    Email = "admin",
                    Password = PasswordHasher.Hash("admin"),
                    FullName = "Admin user"
                };
                
                await userRepository.CreateAsync(user, UserContext.Guest, DataOptions.Bypass);
            }
            
            return roleRepository;
        });
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();
    
    private Func<Task> Noop = () => Task.CompletedTask;
}