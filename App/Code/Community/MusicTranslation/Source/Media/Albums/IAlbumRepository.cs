using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Attributes;
using LLE.Kernel.Security;

namespace LLE.MusicTranslation.Media.Albums;

[Repository(typeof(Album))]
public interface IAlbumRepository : IEntityRepository<Album>
{
    [Query("AlbumKey = :albumKey")]
    Task<Album?> FindByAlbumKeyAsync(string albumKey, UserContext context, DataOptions options);
}