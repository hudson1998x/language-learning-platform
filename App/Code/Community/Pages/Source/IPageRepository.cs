using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;

namespace LLE.Pages;

[Repository(typeof(Page))]
public interface IPageRepository : IEntityRepository<Page>
{
    
}