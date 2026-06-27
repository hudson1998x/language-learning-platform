using LLE.Kernel.Contracts;
using LLE.Kernel.Registry;
using LLE.LLMFramework.Services;
using LLE.ReactFrontend.Events;
using LLE.Sockets.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace LLE.LLMProviders.ChatGPT;

public class ChatGPTModule : IModuleLoader
{
    public Task AppStart()
    {
        Features.LoadFeatures();

        var llmService = ServiceCatalog.GetService<LLMService>();
        llmService.Register<ChatGPTProvider>("ChatGPT");

        Eventing.Eventing.Of<ComponentRegistryGeneratorEvents>().BeforeWrite.Concurrent(registry =>
        {
            registry.AddAutoImport("./App/Code/Community/LLMProviders/ChatGPT/Source/web/configuration/model-selector/index.tsx");
            registry.AddAutoImport("./App/Code/Community/LLMProviders/ChatGPT/Source/web/help/connecting-chatgpt/index.tsx");
        });

        Eventing.Eventing.Of<KestrelHttpEvents>().WebApplication.Concurrent(webApp =>
        {
            var env = webApp.Services.GetRequiredService<IWebHostEnvironment>();

            webApp.UseStaticFiles(new StaticFileOptions()
            {
                RequestPath = "/media/chatgpt/help",
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.ContentRootPath, "./App/Code/Community/LLMProviders/ChatGPT/Source/web/help/connecting-chatgpt/media")
                )
            });
        });

        return Task.CompletedTask;
    }

    public Task AppStop() => Noop();
    public Task Install() => Noop();
    public Task Uninstall() => Noop();

    private Func<Task> Noop = () => Task.CompletedTask;
}
