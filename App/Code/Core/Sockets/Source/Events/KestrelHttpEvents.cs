using LLE.Eventing;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace LLE.Sockets.Events;

/// <summary>
/// Defines the set of events raised by <see cref="HttpSocket"/> while building and starting
/// its underlying Kestrel <see cref="WebApplication"/>, allowing other modules (e.g. an SSL
/// module) to hook in and extend the server without <see cref="HttpSocket"/> needing to know
/// about them directly.
/// </summary>
public class KestrelHttpEvents : EventTable
{
    /// <summary>
    /// Raised after the <see cref="WebApplicationBuilder"/> is created, before any Kestrel
    /// configuration is applied. Subscribers can use this to register services or otherwise
    /// configure the builder.
    /// </summary>
    public readonly EventCollection<WebApplicationBuilder> WebAppBuilder = new();

    /// <summary>
    /// Raised while configuring Kestrel, after the HTTP port listener has been registered.
    /// Subscribers can use this to further customize the server's options as a whole
    /// (e.g. request limits, additional standalone endpoints).
    /// </summary>
    public readonly EventCollection<KestrelServerOptions> ServerOptions = new();

    /// <summary>
    /// Raised with the <see cref="ListenOptions"/> for the HTTP port's listener specifically,
    /// at the point it is registered. This is the hook an SSL module should use to enable
    /// HTTPS on that listener (e.g. by calling <c>listen.UseHttps(...)</c>), without
    /// <see cref="HttpSocket"/> needing any knowledge of certificates or TLS configuration.
    /// </summary>
    public readonly EventCollection<ListenOptions> Listen = new();

    /// <summary>
    /// Raised after the <see cref="WebApplication"/> has been built, before it starts
    /// accepting requests. Subscribers can use this to configure middleware or routing.
    /// </summary>
    public readonly EventCollection<WebApplication> WebApplication = new();
}