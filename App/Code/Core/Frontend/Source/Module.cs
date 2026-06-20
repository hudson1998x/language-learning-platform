using LLE.Frontend.Builders;
using LLE.Sockets.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Source.Contracts;

namespace LLE.Frontend;

public class FrontendModule : IModuleLoader
{
    
    public async Task AppStart()
    {
        // register the serializer for HtmlBuilder.
        Eventing.Eventing.Of<HttpSocketEvents>().Ready.Concurrent((http) =>
        {
            http.AddSerializer<HtmlBuilder>(async (context, builder) =>
            {
                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/html";
                await builder.WriteToStreamAsync(context.Response.BodyWriter);
            });
        });
        Eventing.Eventing.Of<KestrelHttpEvents>().WebApplication.Concurrent((kestrel) =>
        {
            kestrel.MapFallback(async (httpContext) =>
            {
                await FrontendApp(httpContext, true);
            });
            kestrel.MapGet("/", async (context) =>
            {
                await FrontendApp(context);
            });
        });
    }

    private async Task FrontendApp(HttpContext context, bool is404 = false)
    {
        context.Response.StatusCode = 200;
        context.Response.ContentType = "text/html";

        var builder = await HtmlBuilder.Create();

        builder.WithTitle(is404 ? "Page not found" : "Untitled");
        
        await builder.WriteToStreamAsync(context.Response.BodyWriter);
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();
    
    private Func<Task> Noop = () => Task.CompletedTask;
}