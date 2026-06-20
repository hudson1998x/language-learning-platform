using LLE.Kernel.Contracts;

namespace LLE.Auth;

public class AuthModule : IModuleLoader
{
    public async Task AppStart()
    {
        FeatureLoader.LoadFeatures();
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();
    
    private Func<Task> Noop = () => Task.CompletedTask;
}