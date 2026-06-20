using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;

namespace LLE.Auth.Features.Roles;

[Repository(typeof(Role))]
public interface IRoleRepository : IEntityRepository<Role>
{
    
}