using LLE.Kernel.Events;
using LLE.Sockets;
using LLE.TypeScript.Builders;
using LLE.TypeScript.Events;
using ApplicationLoader = LLE.Kernel.ApplicationLoader;

namespace LLE.Application
{
    public static partial class Program
    {
        public static async Task Main(string[] args)
        {
            Eventing.Eventing.Of<NodeEvents>().PackageJson.Concurrent(packageJson =>
            {
                packageJson.AddScript("start", "dotnet run --project App/Code/Core/Application");
            });
            
            var apiBuilder = new ApiBuilder();
            
            Eventing.Eventing.Of<FeatureEvents>().Features.Concurrent(apiBuilder.AddFeature);
            
            ModuleRegistry.AddModules();

            var webServer = new HttpSocket(8080);

            await ApplicationLoader.StartLifecycle();
            
            await Eventing.Eventing.Of<DatabaseEvents>().SeedAllAsync();

            await webServer.StartAsync();
            apiBuilder.WriteToDisk("App/Api");
            apiBuilder.Dispose();
            await webServer.ListenAsync();
        }
    }
}
