using LLE.Frontend.Builders;
using LLE.Kernel.Contracts;
using LLE.Sockets.Events;
using LLE.UiIR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace LLE.Frontend;

public class FrontendModule : IModuleLoader
{
    
    public Task AppStart()
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
        
        // until an await is used, we just return this.
        return Task.CompletedTask;
    }

    private static async Task FrontendApp(HttpContext context, bool is404 = false)
    {
        context.Response.StatusCode = 200;
        context.Response.ContentType = "text/html";
        
        // creates a canvas.
        var builder = await CanvasBuilder.CreateHtmlBuilder(
            async (rootVNode) =>
            {
                if (is404)
                {
                    rootVNode.AddChild(
                        VNode.Create("@component/Text", new Dictionary<string, object>
                        {
                            ["Text"] = "Page not found: " + context.Request.Path
                        })
                    );   
                }
                else
                {
                    rootVNode.AddChild(
                        VNode.Create("@component/Text", new Dictionary<string, object>
                        {
                            ["Text"] = "Homepage"
                        })
                    );   
                }
                
                return rootVNode;
            }    
        );
        
        

        // TODO: Instead of Untitled, put something more useful, potentially
        //  a configuration thing.
        builder.WithTitle(is404 ? "Page not found" : "Untitled");
        
        await builder.WriteToStreamAsync(context.Response.BodyWriter);
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();
    
    private Func<Task> Noop = () => Task.CompletedTask;
}