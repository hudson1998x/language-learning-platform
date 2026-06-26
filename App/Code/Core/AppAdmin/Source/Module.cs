using LLE.Frontend.Builders;
using LLE.Kernel.Contracts;
using LLE.ReactFrontend.Events;
using LLE.Sockets.Events;
using LLE.TypeScript.Events;
using LLE.UiIR;
using Microsoft.AspNetCore.Builder;

namespace LLE.AppAdmin;

public class AppAdminModule : IModuleLoader 
{
    public Task AppStart()
    {
        Features.LoadFeatures();
        Eventing.Eventing.Of<ComponentRegistryGeneratorEvents>().BeforeWrite.Concurrent(tsConfig =>
        {
            Console.WriteLine("Added import");
            tsConfig.AddAutoImport("./App/Code/Core/AppAdmin/Source/web/config-editor/index.tsx");
        });
        
        // create a configuration page. 
        Eventing.Eventing.Of<KestrelHttpEvents>().WebApplication.Concurrent(webApp =>
        {
            webApp.MapGet("/settings", async (context) =>
            {
                context.Response.StatusCode = 200;
                context.Response.ContentType = "text/html";
        
                // creates a canvas.
                var builder = await CanvasBuilder.CreateHtmlBuilder(
                    async (rootVNode) =>
                    {
                        rootVNode.AddChild(
                            VNode.Create("@admin/configuration", [], [])
                        );   
                     
                        return rootVNode;
                    }    
                );
                
                
                builder.WithTitle("Settings");
        
                await builder.WriteToStreamAsync(context.Response.BodyWriter);
            });
        });
        
        return Noop();
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();
    
    private Func<Task> Noop = () => Task.CompletedTask;
}