using LLE.Frontend.Events;
using Source.Contracts;

namespace LLE.ReactFrontend;

public class ReactFrontendModule : IModuleLoader
{
    public Task AppStart()
    {
        Eventing.Eventing.Of<HtmlBuilderEvents>().Body.Concurrent(writer =>
        {
            writer.Append("<div id=\"app\"></div>");
        });
        return Task.CompletedTask;
    }

    public Task AppStop() => Noop();

    public Task Install() => Noop();

    public Task Uninstall() => Noop();
    
    private Func<Task> Noop = () => Task.CompletedTask;
}