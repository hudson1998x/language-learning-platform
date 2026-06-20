using LLE.Kernel.Events;
using LLE.Sockets;
using ApplicationLoader = LLE.Kernel.ApplicationLoader;

namespace LLE.Application
{
    public static partial class Program
    {
        public static async Task Main(string[] args)
        {
            ModuleRegistry.AddModules();

            var webServer = new HttpSocket(8080);

            await ApplicationLoader.StartLifecycle();

            await webServer.StartAsync();
            await webServer.ListenAsync();
        }
    }
}
