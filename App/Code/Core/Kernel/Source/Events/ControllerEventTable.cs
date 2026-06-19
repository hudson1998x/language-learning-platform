using LLE.Eventing;

namespace LLE.Kernel.Events;

public class ControllerEventTable : EventTable
{
    public readonly EventCollection<object[]> ControllersMounted = new();
}