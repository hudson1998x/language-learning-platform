using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.DataQL.Attributes;
using LLE.Kernel.Security;

namespace LLE.LeMessage.Profiles;

[Repository(typeof(Profile))]
public interface IProfileRepository : IEntityRepository<Profile>
{
    [Query("LanguageName = :languageName")]
    Task<List<Profile>> FindByLanguageAsync(string languageName, UserContext context, DataOptions dataOptions, SortOption sort, Pagination pagination);
}
