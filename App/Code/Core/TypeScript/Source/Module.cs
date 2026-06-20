using LLE.Kernel.Contracts;
using LLE.TypeScript.Builders;
using LLE.TypeScript.Events;

namespace LLE.TypeScript;

public class TypeScriptModule : IModuleLoader
{
    public async Task AppStart()
    {
        var tsconfig = new TsConfigBuilder();

        await Eventing.Eventing.Of<TypeScriptEvents>().TsConfig.DispatchAsync(tsconfig);

        await File.WriteAllTextAsync("tsconfig.json", tsconfig.ToString());
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();
    
    private Func<Task> Noop = () => Task.CompletedTask;
}