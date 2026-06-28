using LLE.Kernel.Contracts;
using LLE.Kernel.Registry;
using LLE.LLMFramework.Services;
using LLE.ReactFrontend.Events;
using LLE.Sockets.Events;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace LLE.LLMProviders.Ollama;

public class OllamaModule : IModuleLoader
{
    public Task AppStart()
    {
        Features.LoadFeatures();

        var llmService = ServiceCatalog.GetService<LLMService>();
        llmService.Register<OllamaProvider>("Ollama");

        Eventing.Eventing.Of<ComponentRegistryGeneratorEvents>().BeforeWrite.Concurrent(registry =>
        {
            registry.AddAutoImport("./App/Code/Community/LLMProviders/Ollama/Source/web/configuration/model-selector/index.tsx");
        });

        Eventing.Eventing.Of<KestrelHttpEvents>().WebApplication.Concurrent(webApp =>
        {
            var env = webApp.Services.GetRequiredService<IWebHostEnvironment>();

            webApp.UseStaticFiles(new StaticFileOptions()
            {
                RequestPath = "/media/ollama",
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(env.ContentRootPath, "./App/Code/Community/LLMProviders/Ollama/Source/web")
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
