using LLE.Eventing;

namespace LLE.AppAdmin.Events;

public class ConfigurationEvents : EventTable
{
    public readonly EventCollection<object> Changed = new();
}