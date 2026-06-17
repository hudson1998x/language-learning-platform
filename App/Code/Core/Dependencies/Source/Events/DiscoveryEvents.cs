using LLE.Eventing;

namespace LLE.Dependencies.Events;

public class DiscoveryEvents : EventTable
{
    public readonly EventCollection<Type> Controller = new();
    
    public readonly EventCollection<Type> Entity = new();
    
    public readonly EventCollection<Type> Service = new();
    
    public readonly EventCollection<Type> Provider = new();
    
    public readonly EventCollection<Type> Repository = new();
}