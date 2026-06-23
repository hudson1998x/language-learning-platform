using LLE.Kernel.Contracts;

namespace LLE.LLMFramework;

public class LLMModule : IModuleLoader
{
    public Task AppStart()
    {
        return Task.CompletedTask;
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();

    private Func<Task> Noop = () => Task.CompletedTask;
}
