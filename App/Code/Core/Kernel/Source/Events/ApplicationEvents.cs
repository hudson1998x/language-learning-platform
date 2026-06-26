using LLE.Eventing;

namespace LLE.Kernel.Events;

public class ApplicationEvents : EventTable
{
    public readonly EventCollection<object> AllStarted = new();
}
