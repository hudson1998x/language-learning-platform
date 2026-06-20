using LLE.Eventing;

namespace LLE.Kernel.Events;

public class RepositoryConstructionEvents : EventTable
{
    public readonly EventCollection<RepositoryConstructionContext> Constructed = new();
}
