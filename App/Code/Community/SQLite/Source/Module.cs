using LLE.Kernel.Contracts;
using LLE.Kernel.Events;

namespace LLE.SQLiteAdapter;

public class SQLiteModule : IModuleLoader
{
    public async Task AppStart()
    {
        Eventing.Eventing.Of<RepositoryConstructionEvents>().Constructed.Concurrent(repository =>
        {
            repository.Adapter = new SQLiteAdapter();
        });
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();
    
    private Func<Task> Noop = () => Task.CompletedTask;
}