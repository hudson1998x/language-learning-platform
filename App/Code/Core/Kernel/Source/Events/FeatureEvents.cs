using LLE.Eventing;
using LLE.Kernel.Registry;

namespace LLE.Kernel.Events;

public class FeatureEvents : EventTable
{
    public readonly EventCollection<FeatureDefinition> Features = new();
}