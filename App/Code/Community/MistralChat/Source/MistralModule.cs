using LLE.Kernel.Contracts;
using LLE.Kernel.Registry;
using LLE.LLMFramework.Services;
using LLE.ReactFrontend.Events;

namespace LLE.LLMProviders.MistralChat;

public class MistralModule : IModuleLoader
{
    public Task AppStart()
    {
        Features.LoadFeatures();

        var llmService = ServiceCatalog.GetService<LLMService>();
        llmService.Register<MistralProvider>("MistralChat");

        Eventing.Eventing.Of<ComponentRegistryGeneratorEvents>().BeforeWrite.Concurrent(registry =>
        {
            registry.AddAutoImport("./App/Code/Community/MistralChat/Source/web/configuration/model-selector/index.tsx");
        });

        return Task.CompletedTask;
    }

    public Task AppStop() => Noop();
    public Task Install() => Noop();
    public Task Uninstall() => Noop();

    private Func<Task> Noop = () => Task.CompletedTask;
}
