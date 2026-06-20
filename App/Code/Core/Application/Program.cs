using LLE.Kernel.Events;
using LLE.Sockets;
using LLE.TypeScript.Builders;
using ApplicationLoader = LLE.Kernel.ApplicationLoader;

namespace LLE.Application
{
    public static partial class Program
    {
        public static async Task Main(string[] args)
        {
            var apiBuilder = new ApiBuilder();
            
            Eventing.Eventing.Of<FeatureEvents>().Features.Concurrent(apiBuilder.AddFeature);
            
            ModuleRegistry.AddModules();

            var webServer = new HttpSocket(8080);

            await ApplicationLoader.StartLifecycle();

            await webServer.StartAsync();
            apiBuilder.WriteToDisk("App/Api");
            apiBuilder.Dispose();
            await webServer.ListenAsync();
        }
    }
}
