using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;

namespace LLE.Languages;

[Repository(typeof(Language), IsCached = true, CacheSize = 20)]
public interface ILanguageRepository : IEntityRepository<Language>
{
    
}