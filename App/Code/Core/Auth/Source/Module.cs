using LLE.Kernel.Contracts;
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
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();
    
    private Func<Task> Noop = () => Task.CompletedTask;
}