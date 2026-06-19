using LLE.Kernel.Events;
using LLE.Sockets;
using ApplicationLoader = LLE.Kernel.ApplicationLoader;

namespace LLE.Application
{
    public static partial class Program
    {
        public static async Task Main(string[] args)
        {
            // load all core modules
            ModuleRegistry.AddModules();

            Eventing.Eventing.Of<ControllerEventTable>().ControllersMounted.Concurrent(
                async (controllers) =>
                {
                    await StartServer(controllers);
                    return controllers;
                }
            );
            
            // start the module lifecycle.
            await ApplicationLoader.StartLifecycle();
        }
        private static async Task StartServer(object[] controllers)
        {
            var webServer = new HttpSocket(8080);
            await webServer.StartAsync();
            webServer.LoadControllers(controllers);
            
            await webServer.ListenAsync();
        }
    }
}