using LLE.Kernel.Contracts;
using LLE.Kernel.Registry;
using LLE.LLMFramework.Configurations;
using LLE.ReactFrontend.Events;

namespace LLE.LLMFramework;

public class LLMModule : IModuleLoader
{
    public Task AppStart()
    {
        Features.LoadFeatures();

        ConfigurationCatalog.GetConfiguration<LLMConfiguration>();

        Eventing.Eventing.Of<ComponentRegistryGeneratorEvents>().BeforeWrite.Concurrent(registry =>
        {
            registry.AddAutoImport("./App/Code/Community/LLMFramework/Source/web/configuration/provider-selector/index.tsx");
        });

        return Task.CompletedTask;
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();

    private Func<Task> Noop = () => Task.CompletedTask;
}
