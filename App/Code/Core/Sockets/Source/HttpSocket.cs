using LLE.SharedUtils.Threading;
using LLE.Sockets.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace LLE.Sockets
{
    /// <summary>
    /// Wraps a Kestrel-backed <see cref="WebApplication"/>, managing its lifecycle
    /// (start, stop, dispose) and exposing extension points via <see cref="KestrelHttpEvents"/>
    /// so other modules can configure the builder, the HTTP port's listener (e.g. to enable
    /// HTTPS), Kestrel server options as a whole, and the built app before it starts.
    /// </summary>
    /// <remarks>
    /// <see cref="HttpSocket"/> deliberately has no knowledge of TLS/certificates. To enable
    /// HTTPS, a separate SSL module should subscribe to <see cref="KestrelHttpEvents.Listen"/>
    /// and call <c>listen.UseHttps(...)</c> on the dispatched <see cref="Microsoft.AspNetCore.Server.Kestrel.Core.ListenOptions"/>.
    /// </remarks>
    public sealed partial class HttpSocket
        : IAsyncDisposable
    {
        private readonly int _httpPort;
        private WebApplication? _application;

        public HttpSocket(int httpPort)
        {
            _httpPort = httpPort;
            SubscribeToFeatures();
        }

        public async Task StartAsync(
            CancellationToken cancellationToken = default)
        {
            if (_application is not null)
            {
                throw new InvalidOperationException(
                    "Server already started.");
            }

            var builder = WebApplication.CreateBuilder();

            await Eventing.Eventing.Of<KestrelHttpEvents>().WebAppBuilder.DispatchAsync(builder);

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(
                    _httpPort,
                    listen =>
                    {
                        AsyncUtils.Await(Eventing.Eventing.Of<KestrelHttpEvents>().Listen.DispatchAsync(listen));
                    });

                AsyncUtils.Await(Eventing.Eventing.Of<KestrelHttpEvents>().ServerOptions.DispatchAsync(options));
            });

            var app = builder.Build();

            await Eventing.Eventing.Of<KestrelHttpEvents>().WebApplication.DispatchAsync(app);

            await app.StartAsync(cancellationToken);

            _application = app;

            FlushPendingFeatures(app);

            await Eventing.Eventing.Of<HttpSocketEvents>().Ready.DispatchAsync(this);
        }

        public async Task ListenAsync(CancellationToken cancellationToken = default)
        {
            if (_application is not null)
            {
                await _application.WaitForShutdownAsync(cancellationToken);   
            }
        }

        public async Task StopAsync(
            CancellationToken cancellationToken = default)
        {
            if (_application is null)
            {
                return;
            }

            await _application.StopAsync(cancellationToken);
            await _application.DisposeAsync();

            _application = null;
        }

        public async ValueTask DisposeAsync()
        {
            await StopAsync();
        }
    }
}
