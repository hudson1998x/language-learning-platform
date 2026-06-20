using LLE.Eventing;

namespace LLE.Kernel.Events;

public class EntityEvents<T> : EventTable
{
    public readonly EventCollection<T> BeforeCreate = new();
    public readonly EventCollection<T> AfterCreate = new();
    public readonly EventCollection<T> BeforeUpdate = new();
    public readonly EventCollection<T> AfterUpdate = new();
    public readonly EventCollection<T> BeforeDelete = new();
    public readonly EventCollection<T> AfterDelete = new();
}