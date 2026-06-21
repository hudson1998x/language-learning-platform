using LLE.Kernel.Attributes;
using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Attributes;
using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.Security;
using Tests.Builders.RepositoryProxyTests.Fakes;

namespace Tests.Builders.RepositoryProxyTests;

public interface ITestRepository : IEntityRepository<TestEntity>
{
    string Name { get; set; }
}

[Repository(typeof(TestEntity), IsCached = true, CacheSize = 10)]
public interface ICachedTestRepository : IEntityRepository<TestEntity>
{
}

public interface ITestQueryRepository : IEntityRepository<TestEntity>
{
    [Query("Name = :name")]
    Task<List<TestEntity>> FindByNameAsync(string name, UserContext context, DataOptions options);

    [Query("Status = :status")]
    Task<List<TestEntity>> FindByStatusAsync(string status, UserContext context, DataOptions options, SortOption? sortBy = null, Pagination? pagination = null);

    [Query("IsDeleted = false")]
    Task<List<TestEntity>> FindActiveAsync(UserContext context, DataOptions options);
}

public interface INonRepositoryInterface
{
    void DoSomething();
}

public interface ITestRepositoryNoId : IEntityRepository<TestEntityWithoutId>
{
}

public interface IInvalidMethodRepository : IEntityRepository<TestEntity>
{
    Task<int> UnsupportedMethodAsync();
}

public interface IRepositoryWithOnlyExecuteQuery : IDatabaseAdapter
{
}
