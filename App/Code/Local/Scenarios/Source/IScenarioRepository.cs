using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.Security;
using LLE.Kernel.DataQL.Attributes;

namespace LLE.Scenarios;

[Repository(typeof(Scenario))]
public interface IScenarioRepository : IEntityRepository<Scenario>
{
    [Query("Title like :title")]
    public Task<List<Scenario>> SearchByTitle(string title, UserContext context, DataOptions dataOptions, SortOption sort, Pagination pagination);
    
    [Query("Steps like :steps")]
    public Task<List<Scenario>> SearchBySteps(string steps, UserContext context, DataOptions dataOptions, SortOption sort, Pagination pagination);
}