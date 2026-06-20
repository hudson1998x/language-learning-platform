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
    /// <param name="httpPort">The port to listen on.</param>
    public sealed partial class HttpSocket(
        int httpPort)
        : IAsyncDisposable
    {
        /// <summary>
        /// The running <see cref="WebApplication"/> instance, or <c>null</c> if the server
        /// has not been started (or has been stopped).
        /// </summary>
        private WebApplication? _application;

        /// <summary>
        /// Builds and starts the underlying <see cref="WebApplication"/>, configuring Kestrel
        /// to listen on the configured HTTP port. Other modules (e.g. SSL) can hook into the
        /// <see cref="KestrelHttpEvents"/> dispatched during this process to extend the server.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the startup operation.</param>
        /// <exception cref="InvalidOperationException">Thrown if the server has already been started.</exception>
        public async Task StartAsync(
            CancellationToken cancellationToken = default)
        {
            if (_application is not null)
            {
                throw new InvalidOperationException(
                    "Server already started.");
            }
            
            SubscribeToFeatures();

            var builder = WebApplication.CreateBuilder();

            // Allow external subscribers to configure the WebApplicationBuilder before
            // Kestrel options or the app itself are set up (e.g. registering services).
            await Eventing.Eventing.Of<KestrelHttpEvents>().WebAppBuilder.DispatchAsync(builder);

            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenAnyIP(
                    httpPort,
                    listen =>
                    {
                        // Hand this specific listener off to subscribers (e.g. an SSL module),
                        // which can call listen.UseHttps(...) to enable TLS on this port.
                        // HttpSocket itself has no knowledge of certificates.
                        AsyncUtils.Await(Eventing.Eventing.Of<KestrelHttpEvents>().Listen.DispatchAsync(listen));
                    });

                // Allow external subscribers to further customize Kestrel's server options
                // as a whole (e.g. request limits, additional endpoints) after the listener is set.
                AsyncUtils.Await(Eventing.Eventing.Of<KestrelHttpEvents>().ServerOptions.DispatchAsync(options));
            });

            var app = builder.Build();
        
            // Allow external subscribers to configure the built WebApplication
            // (e.g. middleware, routing) before it starts accepting requests.
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

        /// <summary>
        /// Stops the underlying <see cref="WebApplication"/> and disposes it. Safe to call
        /// even if the server was never started or has already been stopped, in which case
        /// this is a no-op.
        /// </summary>
        /// <param name="cancellationToken">A token to cancel the shutdown operation.</param>
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

        /// <summary>
        /// Stops and disposes the underlying <see cref="WebApplication"/> if it is running.
        /// Equivalent to calling <see cref="StopAsync"/> with the default cancellation token.
        /// </summary>
        public async ValueTask DisposeAsync()
        {
            await StopAsync();
        }
    }
}