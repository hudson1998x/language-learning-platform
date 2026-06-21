using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Attributes;
using LLE.Kernel.Security;

namespace LLE.Auth.Features.Users;

[Repository(typeof(User))]
public interface IUserRepository : IEntityRepository<User>
{
    [Query("Email = :email")]
    public Task<User?> GetByEmailAsync(string email, UserContext context, DataOptions options);
}