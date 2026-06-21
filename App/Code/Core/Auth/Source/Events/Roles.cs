using LLE.Auth.Features.Roles;
using LLE.Eventing;

namespace LLE.Auth.Events;

public class RolesEventTable : EventTable
{
    public readonly EventCollection<IRoleRepository> Setup = new();
    
    public readonly EventCollection<IRoleRepository> Ready = new();
}