using LLE.Kernel.Contracts;
using LLE.LLMFramework.Services;

namespace LLE.LLMProviders.Ollama;

public class OllamaModule : IModuleLoader
{
    public Task AppStart()
    {
        LLMService.Register<OllamaProvider>("Ollama");
        return Task.CompletedTask;
    }

    public Task AppStop() => Noop();
    public Task Install() => Noop();
    public Task Uninstall() => Noop();

    private Func<Task> Noop = () => Task.CompletedTask;
}
