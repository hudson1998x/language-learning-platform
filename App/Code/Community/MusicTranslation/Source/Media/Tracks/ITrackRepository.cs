using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;

namespace LLE.MusicTranslation.Media.Tracks;

[Repository(typeof(Track))]
public interface ITrackRepository : IEntityRepository<Track>
{
    
}