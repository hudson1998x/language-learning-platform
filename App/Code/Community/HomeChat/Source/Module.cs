using LLE.Kernel.Contracts;

namespace LLE.HomeChat;

public class HomeChatModule : IModuleLoader
{
    public Task AppStart()
    {
        Features.LoadFeatures();
        return Task.CompletedTask;
    }

    public Task AppStop() => Task.CompletedTask;
    public Task Install() => Task.CompletedTask;
    public Task Uninstall() => Task.CompletedTask;
}
