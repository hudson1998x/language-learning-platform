using System.Text.Json;
using LLE.Frontend.Builders;
using LLE.Kernel.AutoEntity;
using LLE.Kernel.Contracts;
using LLE.Kernel.Events;
using LLE.Kernel.Registry;
using LLE.Kernel.Security;
using LLE.Sockets.Events;
using LLE.UiIR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace LLE.Pages;

public class PagesModule : IModuleLoader
{
    public Task AppStart()
    {
        AutoEntityFeature.AutoFeature<Page, IPageRepository>();

        Eventing.Eventing.Of<KestrelHttpEvents>().WebApplication.Concurrent(async builder =>
        {
            var pageRepository = RepositoryCatalog.GetRepository<IPageRepository>();

            var allPages = await pageRepository.FindAllAsync(UserContext.Guest, DataOptions.Bypass);

            foreach (var page in allPages)
            {
                builder.MapGet(page.Url, async context =>
                {
                    var canvasNode = JsonSerializer.Deserialize<VNode>(page.PageJson);

                    if (canvasNode is null)
                    {
                        context.Response.StatusCode = 404;
                        context.Response.ContentType = "text/plain";
                        await context.Response.WriteAsync("Issue reading page");
                        return;
                    }

                    var canvasBuilder = await CanvasBuilder.CreateHtmlBuilder(async (vnode) =>
                    {
                        vnode.Change(canvasNode);
                        return vnode;
                    });

                    canvasBuilder.WithTitle(page.Title + " | LLE");
                    
                    context.Response.ContentType = "text/html";
                    context.Response.StatusCode = 200;
                    await canvasBuilder.WriteToStreamAsync(context.Response.BodyWriter);
                });
            }
        });
        
        return Task.CompletedTask;
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();
    
    private Func<Task> Noop = () => Task.CompletedTask;
}