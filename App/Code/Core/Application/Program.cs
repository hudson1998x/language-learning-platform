
using LLE.Sockets;

namespace LLE.Application
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {


            var webServer = new HttpSocket(8080);
            await webServer.StartAsync();
            
            await webServer.ListenAsync();
        }
    }
}