using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Attributes;
using LLE.Kernel.Security;

namespace LLE.Auth.Features.Roles;

[Repository(typeof(Role), IsCached = true, CacheSize = 10)]
public interface IRoleRepository : IEntityRepository<Role>
{
    [Query("Key = :key")]
    public Task<Role> FindByKeyAsync(string key, UserContext userContext, DataOptions dataOptions);
}