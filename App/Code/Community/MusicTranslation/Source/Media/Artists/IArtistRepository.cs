using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Attributes;
using LLE.Kernel.Security;

namespace LLE.MusicTranslation.Media.Artists;

[Repository(typeof(Artist))]
public interface IArtistRepository : IEntityRepository<Artist>
{
    [Query("Name = :name")]
    Task<Artist?> FindByNameAsync(string name, UserContext context, DataOptions options);
}