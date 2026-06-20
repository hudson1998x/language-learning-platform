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
            
            // start the module lifecycle.
            await ApplicationLoader.StartLifecycle();

            await StartServer();
        }
        private static async Task StartServer()
        {
            var webServer = new HttpSocket(8080);
            await webServer.StartAsync();
            
            await webServer.ListenAsync();
        }
    }
}