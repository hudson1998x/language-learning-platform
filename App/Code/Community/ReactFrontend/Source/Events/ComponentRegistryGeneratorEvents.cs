using LLE.Eventing;
using LLE.ReactFrontend.Generators;

namespace LLE.ReactFrontend.Events;

public class ComponentRegistryGeneratorEvents : EventTable
{
    public readonly EventCollection<ComponentRegistryGenerator> BeforeWrite = new();
}