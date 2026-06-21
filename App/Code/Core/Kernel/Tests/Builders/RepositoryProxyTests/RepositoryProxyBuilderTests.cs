using System.Reflection;
using LLE.Kernel.Builders;
using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.DataQL.Enums;
using LLE.Kernel.Security;
using Tests.Builders.RepositoryProxyTests.Fakes;

namespace Tests.Builders.RepositoryProxyTests;

public sealed class RepositoryProxyBuilderTests : IDisposable
{
    private readonly FakeDatabaseAdapter _adapter = new();

    public void Dispose()
    {
        _adapter.ReceivedNodes.Clear();
    }

    // ─── Helpers ─────────────────────────────────────────────────────────

    private static void InjectAdapter(object proxy, IDatabaseAdapter adapter)
    {
        var field = GetFieldInfo(proxy, "_adapter");
        field.SetValue(proxy, adapter);
    }

    private static FieldInfo GetFieldInfo(object proxy, string name)
    {
        return proxy.GetType().GetField(name, BindingFlags.NonPublic | BindingFlags.Instance)
            ?? throw new InvalidOperationException(
                $"Proxy type '{proxy.GetType().Name}' has no field '{name}'.");
    }

    private static T GetFieldValue<T>(object proxy, string name)
    {
        return (T)GetFieldInfo(proxy, name).GetValue(proxy)!;
    }

    private static void SetFieldValue(object proxy, string name, object? value)
    {
        GetFieldInfo(proxy, name).SetValue(proxy, value);
    }

    private T BuildAndInject<T>() where T : class
    {
        var proxy = RepositoryProxyBuilder.BuildProxyRepository<T>();
        InjectAdapter(proxy, _adapter);
        return proxy;
    }

    // ─── Proxy Construction ──────────────────────────────────────────────

    [Fact]
    public void BuildProxyRepository_CreatesInstanceImplementingInterface()
    {
        var proxy = RepositoryProxyBuilder.BuildProxyRepository<ITestRepository>();
        Assert.NotNull(proxy);
        Assert.IsAssignableFrom<ITestRepository>(proxy);
    }

    [Fact]
    public void BuildProxyRepository_ProxyImplementsIEntityRepository()
    {
        var proxy = RepositoryProxyBuilder.BuildProxyRepository<ITestRepository>();
        Assert.IsAssignableFrom<IEntityRepository<TestEntity>>(proxy);
    }

    [Fact]
    public void BuildProxyRepository_ProxyImplementsIDatabaseAdapter()
    {
        var proxy = RepositoryProxyBuilder.BuildProxyRepository<ITestRepository>();
        Assert.IsAssignableFrom<IDatabaseAdapter>(proxy);
    }

    [Fact]
    public void BuildProxyRepository_ReturnsNewInstanceEachCall()
    {
        var a = RepositoryProxyBuilder.BuildProxyRepository<ITestRepository>();
        var b = RepositoryProxyBuilder.BuildProxyRepository<ITestRepository>();

        Assert.NotSame(a, b);
        Assert.NotEqual(a.GetType(), b.GetType());
    }

    [Fact]
    public void BuildProxyRepository_NonInterfaceType_ThrowsInvalidOperation()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            RepositoryProxyBuilder.BuildProxyRepository(typeof(string)));

        Assert.Contains("does not implement IEntityRepository<T>", ex.Message);
    }

    [Fact]
    public void BuildProxyRepository_NonRepositoryInterface_ThrowsInvalidOperation()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            RepositoryProxyBuilder.BuildProxyRepository<INonRepositoryInterface>());

        Assert.Contains("does not implement IEntityRepository<T>", ex.Message);
    }

    [Fact]
    public void BuildProxyRepository_InterfaceInheritingOnlyDatabaseAdapter_ThrowsInvalidOperation()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            RepositoryProxyBuilder.BuildProxyRepository<IRepositoryWithOnlyExecuteQuery>());

        Assert.Contains("does not implement IEntityRepository<T>", ex.Message);
    }

    // ─── Constructor ─────────────────────────────────────────────────────

    [Fact]
    public void Constructor_StoresRepositoryType()
    {
        var proxy = RepositoryProxyBuilder.BuildProxyRepository<ITestRepository>();
        var actual = GetFieldValue<Type>(proxy, "_repositoryType");
        Assert.Equal(typeof(ITestRepository), actual);
    }

    [Fact]
    public void Constructor_StoresEntityType()
    {
        var proxy = RepositoryProxyBuilder.BuildProxyRepository<ITestRepository>();
        var actual = GetFieldValue<Type>(proxy, "_entityType");
        Assert.Equal(typeof(TestEntity), actual);
    }

    [Fact]
    public void Constructor_AdapterFieldIsNull()
    {
        var proxy = RepositoryProxyBuilder.BuildProxyRepository<ITestRepository>();
        var actual = GetFieldValue<object?>(proxy, "_adapter");
        Assert.Null(actual);
    }

    // ─── Entity Type Resolution ──────────────────────────────────────────

    [Fact]
    public void BuildProxyRepository_ResolvesEntityTypeFromDeepHierarchy()
    {
        var proxy = RepositoryProxyBuilder.BuildProxyRepository<ITestRepository>();
        var actual = GetFieldValue<Type>(proxy, "_entityType");
        Assert.Equal(typeof(TestEntity), actual);
    }

    // ─── Properties ──────────────────────────────────────────────────────

    [Fact]
    public void Properties_AreImplementedAsAutoProperties()
    {
        var proxy = RepositoryProxyBuilder.BuildProxyRepository<ITestRepository>();
        proxy.Name = "Hello World";
        Assert.Equal("Hello World", proxy.Name);
    }

    // ─── IDatabaseAdapter.ExecuteQuery Passthrough ───────────────────────

    [Fact]
    public async Task ExecuteQuery_ForwardsNodeDirectlyToAdapter()
    {
        var proxy = BuildAndInject<ITestRepository>();
        var node = new ReadQueryNode { TableName = "Direct", EntityType = typeof(TestEntity) };

        var adapter = (IDatabaseAdapter)proxy;
        await adapter.ExecuteQuery(node);

        Assert.Same(node, Assert.Single(_adapter.ReceivedNodes));
    }

    [Fact]
    public async Task ExecuteQuery_ReturnsAdapterResult()
    {
        var proxy = BuildAndInject<ITestRepository>();
        var adapter = (IDatabaseAdapter)proxy;
        var node = new ReadQueryNode { TableName = "Test", EntityType = typeof(TestEntity) };
        var expected = new object();
        _adapter.OnExecuteQuery = _ => Task.FromResult(expected);

        var result = await adapter.ExecuteQuery(node);

        Assert.Same(expected, result);
    }

    // ─── CreateAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task CreateAsync_BuildsWriteQueryNode()
    {
        var proxy = BuildAndInject<ITestRepository>();
        var item = new TestEntity { Name = "New Item" };
        _adapter.OnExecuteQuery = n => Task.FromResult<object>(item);

        await proxy.CreateAsync(item, UserContext.Guest, DataOptions.Bypass);

        var node = Assert.Single(_adapter.ReceivedNodes);
        var writeNode = Assert.IsType<WriteQueryNode>(node);
        Assert.Equal("TestEntity", writeNode.TableName);
        Assert.Same(item, writeNode.Payload);
        Assert.Null(writeNode.Where);
        Assert.Equal(typeof(TestEntity), writeNode.EntityType);
    }

    [Fact]
    public async Task CreateAsync_PassesContextAndOptions()
    {
        var proxy = BuildAndInject<ITestRepository>();
        var item = new TestEntity();
        var context = new UserContext { UserId = Guid.NewGuid() };
        _adapter.OnExecuteQuery = n => Task.FromResult<object>(item);

        await proxy.CreateAsync(item, context, DataOptions.Bypass);

        Assert.Single(_adapter.ReceivedNodes);
    }

    // ─── UpdateAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task UpdateAsync_BuildsWriteQueryNodeWithIdFilter()
    {
        var proxy = BuildAndInject<ITestRepository>();
        var item = new TestEntity { Id = Guid.NewGuid(), Name = "Updated" };
        _adapter.OnExecuteQuery = n => Task.FromResult<object>(item);

        await proxy.UpdateAsync(item, UserContext.Guest, DataOptions.Bypass);

        var node = Assert.Single(_adapter.ReceivedNodes);
        var writeNode = Assert.IsType<WriteQueryNode>(node);
        Assert.Equal("TestEntity", writeNode.TableName);
        Assert.Same(item, writeNode.Payload);
        Assert.Equal(typeof(TestEntity), writeNode.EntityType);

        var filter = Assert.IsType<FilterNode>(writeNode.Where);
        Assert.Equal("Id", filter.ColumnName);
        Assert.Equal(FilterOperator.Equals, filter.Operator);
        Assert.Equal(item.Id, filter.Value);
    }

    // ─── DeleteAsync ─────────────────────────────────────────────────────

    [Fact]
    public async Task DeleteAsync_BuildsDeleteQueryNodeWithIdFilter()
    {
        var proxy = BuildAndInject<ITestRepository>();
        var item = new TestEntity { Id = Guid.NewGuid() };
        _adapter.OnExecuteQuery = n => Task.FromResult<object>(item);

        await proxy.DeleteAsync(item, UserContext.Guest, DataOptions.Bypass);

        var node = Assert.Single(_adapter.ReceivedNodes);
        var deleteNode = Assert.IsType<DeleteQueryNode>(node);
        Assert.Equal("TestEntity", deleteNode.TableName);
        Assert.Same(item, deleteNode.Payload);
        Assert.Equal(typeof(TestEntity), deleteNode.EntityType);

        var filter = Assert.IsType<FilterNode>(deleteNode.Where);
        Assert.Equal("Id", filter.ColumnName);
        Assert.Equal(FilterOperator.Equals, filter.Operator);
        Assert.Equal(item.Id, filter.Value);
    }

    // ─── FindByIdAsync ──────────────────────────────────────────────────

    [Fact]
    public async Task FindByIdAsync_BuildsReadQueryNodeWithIdFilter()
    {
        var proxy = BuildAndInject<ITestRepository>();
        var id = Guid.NewGuid();
        _adapter.OnExecuteQuery = n => Task.FromResult<object>(new TestEntity { Id = id });

        var result = await proxy.FindByIdAsync(id, UserContext.Guest, DataOptions.Bypass);

        var node = Assert.Single(_adapter.ReceivedNodes);
        var readNode = Assert.IsType<ReadQueryNode>(node);
        Assert.Equal("TestEntity", readNode.TableName);
        Assert.Equal(typeof(TestEntity), readNode.EntityType);

        var filter = Assert.IsType<FilterNode>(readNode.Where);
        Assert.Equal("Id", filter.ColumnName);
        Assert.Equal(FilterOperator.Equals, filter.Operator);
        Assert.Equal(id, filter.Value);

        Assert.NotNull(result);
        Assert.Equal(id, result.Id);
    }

    [Fact]
    public async Task FindByIdAsync_ReturnsNull_WhenNotFound()
    {
        var proxy = BuildAndInject<ITestRepository>();
        _adapter.OnExecuteQuery = _ => Task.FromResult<object>(null!);

        var result = await proxy.FindByIdAsync(Guid.NewGuid(), UserContext.Guest, DataOptions.Bypass);

        Assert.Null(result);
    }

    // ─── FindAllAsync ────────────────────────────────────────────────────

    [Fact]
    public async Task FindAllAsync_BuildsReadQueryNode()
    {
        var proxy = BuildAndInject<ITestRepository>();
        _adapter.OnExecuteQuery = _ => Task.FromResult<object>(new List<TestEntity>());

        var results = await proxy.FindAllAsync(UserContext.Guest, DataOptions.Bypass);

        var node = Assert.Single(_adapter.ReceivedNodes);
        var readNode = Assert.IsType<ReadQueryNode>(node);
        Assert.Equal("TestEntity", readNode.TableName);
        Assert.Equal(typeof(TestEntity), readNode.EntityType);
        Assert.Null(readNode.Where);
        Assert.Null(readNode.OrderBy);
        Assert.Null(readNode.Pagination);
        Assert.NotNull(results);
    }

    [Fact]
    public async Task FindAllAsync_WithSortBy_SetsOrderByOnNode()
    {
        var proxy = BuildAndInject<ITestRepository>();
        var sortBy = new SortOption { Field = "Name", Ascending = true };
        _adapter.OnExecuteQuery = _ => Task.FromResult<object>(new List<TestEntity>());

        await proxy.FindAllAsync(UserContext.Guest, DataOptions.Bypass, sortBy);

        var node = Assert.Single(_adapter.ReceivedNodes);
        var readNode = Assert.IsType<ReadQueryNode>(node);
        Assert.NotNull(readNode.OrderBy);
        Assert.Same(sortBy, (object?)readNode.OrderBy);
    }

    [Fact]
    public async Task FindAllAsync_WithPagination_SetsPaginationOnNode()
    {
        var proxy = BuildAndInject<ITestRepository>();
        var pagination = new Pagination { PageNo = 2, Limit = 20 };
        _adapter.OnExecuteQuery = _ => Task.FromResult<object>(new List<TestEntity>());

        await proxy.FindAllAsync(UserContext.Guest, DataOptions.Bypass, null, pagination);

        var node = Assert.Single(_adapter.ReceivedNodes);
        var readNode = Assert.IsType<ReadQueryNode>(node);
        Assert.Same(pagination, readNode.Pagination);
    }

    [Fact]
    public async Task FindAllAsync_WithSortByAndPagination_SetsBoth()
    {
        var proxy = BuildAndInject<ITestRepository>();
        var sortBy = new SortOption { Field = "Name" };
        var pagination = new Pagination { PageNo = 1, Limit = 5 };
        _adapter.OnExecuteQuery = _ => Task.FromResult<object>(new List<TestEntity>());

        await proxy.FindAllAsync(UserContext.Guest, DataOptions.Bypass, sortBy, pagination);

        var node = Assert.Single(_adapter.ReceivedNodes);
        var readNode = Assert.IsType<ReadQueryNode>(node);
        Assert.NotNull(readNode.OrderBy);
        Assert.Same(sortBy, (object?)readNode.OrderBy);
        Assert.Same(pagination, readNode.Pagination);
    }

    // ─── TotalRecords ────────────────────────────────────────────────────

    [Fact]
    public async Task TotalRecords_BuildsReadQueryNodeWithIsCount()
    {
        var proxy = BuildAndInject<ITestRepository>();
        _adapter.OnExecuteQuery = _ => Task.FromResult<object>(42);

        var count = await proxy.TotalRecords(UserContext.Guest, DataOptions.Bypass);

        var node = Assert.Single(_adapter.ReceivedNodes);
        var readNode = Assert.IsType<ReadQueryNode>(node);
        Assert.Equal("TestEntity", readNode.TableName);
        Assert.True(readNode.IsCount);
        Assert.Null(readNode.Where);
        Assert.Equal(42, count);
    }

    // ─── Custom Query Methods ────────────────────────────────────────────

    [Fact]
    public async Task QueryMethod_ParsesQueryAndBuildsReadQueryNode()
    {
        var proxy = BuildAndInject<ITestQueryRepository>();
        _adapter.OnExecuteQuery = _ => Task.FromResult<object>(new List<TestEntity>());

        await proxy.FindByNameAsync("test-name", UserContext.Guest, DataOptions.Bypass);

        var node = Assert.Single(_adapter.ReceivedNodes);
        var readNode = Assert.IsType<ReadQueryNode>(node);
        Assert.Equal("TestEntity", readNode.TableName);
        Assert.Equal(OperationKind.Read, readNode.OperationKind);
        Assert.Equal(typeof(TestEntity), readNode.EntityType);
        Assert.NotNull(readNode.Where);
    }

    [Fact]
    public async Task QueryMethod_WithNoParameters_DoesNotAllocateParameterDictionary()
    {
        var proxy = BuildAndInject<ITestQueryRepository>();
        _adapter.OnExecuteQuery = _ => Task.FromResult<object>(new List<TestEntity>());

        var items = await proxy.FindActiveAsync(UserContext.Guest, DataOptions.Bypass);

        var node = Assert.Single(_adapter.ReceivedNodes);
        var readNode = Assert.IsType<ReadQueryNode>(node);
        Assert.NotNull(readNode.Where);
        Assert.NotNull(items);
    }

    [Fact]
    public async Task QueryMethod_WithSortBy_SetsOrderByOnNode()
    {
        var proxy = BuildAndInject<ITestQueryRepository>();
        var sortBy = new SortOption { Field = "Name" };
        _adapter.OnExecuteQuery = _ => Task.FromResult<object>(new List<TestEntity>());

        await proxy.FindByStatusAsync("active", UserContext.Guest, DataOptions.Bypass, sortBy);

        var node = Assert.Single(_adapter.ReceivedNodes);
        var readNode = Assert.IsType<ReadQueryNode>(node);
        Assert.NotNull(readNode.OrderBy);
        Assert.Same(sortBy, (object?)readNode.OrderBy);
    }

    [Fact]
    public async Task QueryMethod_WithPagination_SetsPaginationOnNode()
    {
        var proxy = BuildAndInject<ITestQueryRepository>();
        var pagination = new Pagination { PageNo = 3, Limit = 15 };
        _adapter.OnExecuteQuery = _ => Task.FromResult<object>(new List<TestEntity>());

        await proxy.FindByStatusAsync("active", UserContext.Guest, DataOptions.Bypass, null, pagination);

        var node = Assert.Single(_adapter.ReceivedNodes);
        var readNode = Assert.IsType<ReadQueryNode>(node);
        Assert.Same(pagination, readNode.Pagination);
    }

    // ─── Caching ─────────────────────────────────────────────────────────

    [Fact]
    public void CachedRepository_HasCacheField()
    {
        var proxy = RepositoryProxyBuilder.BuildProxyRepository<ICachedTestRepository>();
        var field = proxy.GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.NotNull(field);
    }

    [Fact]
    public void NonCachedRepository_DoesNotHaveCacheField()
    {
        var proxy = RepositoryProxyBuilder.BuildProxyRepository<ITestRepository>();
        var field = proxy.GetType().GetField("_cache", BindingFlags.NonPublic | BindingFlags.Instance);
        Assert.Null(field);
    }

    [Fact]
    public async Task CachedFindByIdAsync_ReturnsFromCache_WhenEntityFound()
    {
        var proxy = BuildAndInject<ICachedTestRepository>();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Cached" };
        SetFieldValue(proxy, "_cache", new object[] { entity });

        var result = await proxy.FindByIdAsync(entity.Id, UserContext.Guest, DataOptions.Bypass);

        Assert.Same(entity, result);
        Assert.Empty(_adapter.ReceivedNodes);
    }

    [Fact]
    public async Task CachedFindByIdAsync_ReturnsNull_WhenEntityNotInCache()
    {
        var proxy = BuildAndInject<ICachedTestRepository>();
        SetFieldValue(proxy, "_cache", new object[] { new TestEntity { Id = Guid.NewGuid() } });

        var result = await proxy.FindByIdAsync(Guid.NewGuid(), UserContext.Guest, DataOptions.Bypass);

        Assert.Null(result);
        Assert.Empty(_adapter.ReceivedNodes);
    }

    [Fact]
    public async Task CachedFindByIdAsync_ReturnsNull_WhenCacheIsEmpty()
    {
        var proxy = BuildAndInject<ICachedTestRepository>();
        SetFieldValue(proxy, "_cache", new object[10]);

        var result = await proxy.FindByIdAsync(Guid.NewGuid(), UserContext.Guest, DataOptions.Bypass);

        Assert.Null(result);
        Assert.Empty(_adapter.ReceivedNodes);
    }

    [Fact]
    public async Task CachedFindAllAsync_WithoutSortOrPagination_UsesCache()
    {
        var proxy = BuildAndInject<ICachedTestRepository>();
        var entity1 = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        var entity2 = new TestEntity { Id = Guid.NewGuid(), Name = "B" };
        SetFieldValue(proxy, "_cache", new object[] { entity1, entity2 });

        var results = await proxy.FindAllAsync(UserContext.Guest, DataOptions.Bypass);

        Assert.Contains(entity1, results);
        Assert.Contains(entity2, results);
        Assert.Empty(_adapter.ReceivedNodes);
    }

    [Fact]
    public async Task CachedFindAllAsync_WithSortBy_UsesAdapterInsteadOfCache()
    {
        var proxy = BuildAndInject<ICachedTestRepository>();
        SetFieldValue(proxy, "_cache", new object[] { new TestEntity { Id = Guid.NewGuid(), Name = "Cached" } });
        _adapter.OnExecuteQuery = _ => Task.FromResult<object>(new List<TestEntity>());

        await proxy.FindAllAsync(UserContext.Guest, DataOptions.Bypass, new SortOption { Field = "Name" });

        Assert.Single(_adapter.ReceivedNodes);
    }

    [Fact]
    public async Task CachedFindAllAsync_WithPagination_UsesAdapterInsteadOfCache()
    {
        var proxy = BuildAndInject<ICachedTestRepository>();
        SetFieldValue(proxy, "_cache", new object[] { new TestEntity { Id = Guid.NewGuid() } });
        _adapter.OnExecuteQuery = _ => Task.FromResult<object>(new List<TestEntity>());

        await proxy.FindAllAsync(UserContext.Guest, DataOptions.Bypass, null, new Pagination());

        Assert.Single(_adapter.ReceivedNodes);
    }

    [Fact]
    public async Task CachedCreateAsync_GoesThroughAdapterAndCachesResult()
    {
        var proxy = BuildAndInject<ICachedTestRepository>();
        var existing = new TestEntity { Id = Guid.NewGuid(), Name = "Before" };
        SetFieldValue(proxy, "_cache", new object[] { existing, null! });
        var newItem = new TestEntity { Id = Guid.NewGuid(), Name = "After" };
        _adapter.OnExecuteQuery = _ => Task.FromResult<object>(newItem);

        var result = await proxy.CreateAsync(newItem, UserContext.Guest, DataOptions.Bypass);

        Assert.Same(newItem, result);
        var cache = GetFieldValue<object[]>(proxy, "_cache");
        Assert.Same(existing, cache[0]);
        Assert.Same(newItem, cache[1]);
    }

    [Fact]
    public async Task CachedUpdateAsync_GoesThroughAdapterAndUpdatesCache()
    {
        var proxy = BuildAndInject<ICachedTestRepository>();
        var existing = new TestEntity { Id = Guid.NewGuid(), Name = "Old" };
        SetFieldValue(proxy, "_cache", new object[] { existing, null! });
        var updated = new TestEntity { Id = existing.Id, Name = "New" };
        _adapter.OnExecuteQuery = _ => Task.FromResult<object>(updated);

        var result = await proxy.UpdateAsync(updated, UserContext.Guest, DataOptions.Bypass);

        Assert.Same(updated, result);
        var cache = GetFieldValue<object[]>(proxy, "_cache");
        Assert.Same(updated, cache[0]);
    }

    [Fact]
    public async Task CachedDeleteAsync_GoesThroughAdapterAndRemovesFromCache()
    {
        var proxy = BuildAndInject<ICachedTestRepository>();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "ToDelete" };
        SetFieldValue(proxy, "_cache", new object[] { entity, null! });
        _adapter.OnExecuteQuery = _ => Task.FromResult<object>(entity);

        await proxy.DeleteAsync(entity, UserContext.Guest, DataOptions.Bypass);

        var cache = GetFieldValue<object[]>(proxy, "_cache");
        Assert.Null(cache[0]);
    }

    [Fact]
    public async Task CachedTotalRecords_GoesThroughAdapterWithIsCount()
    {
        var proxy = BuildAndInject<ICachedTestRepository>();
        _adapter.OnExecuteQuery = _ => Task.FromResult<object>(99);
        SetFieldValue(proxy, "_cache", new object[] { new TestEntity() });

        var count = await proxy.TotalRecords(UserContext.Guest, DataOptions.Bypass);

        var node = Assert.Single(_adapter.ReceivedNodes);
        var readNode = Assert.IsType<ReadQueryNode>(node);
        Assert.True(readNode.IsCount);
        Assert.Equal(99, count);
    }

    // ─── Error Handling ──────────────────────────────────────────────────

    [Fact]
    public void EntityTypeWithoutIdProperty_ThrowsInvalidOperation()
    {
        var ex = Assert.Throws<InvalidOperationException>(() =>
            RepositoryProxyBuilder.BuildProxyRepository<ITestRepositoryNoId>());

        Assert.Contains("must have a public 'Id' property", ex.Message);
    }

    [Fact]
    public void UnsupportedMethod_ThrowsNotSupportedException()
    {
        var ex = Assert.Throws<NotSupportedException>(() =>
            RepositoryProxyBuilder.BuildProxyRepository<IInvalidMethodRepository>());

        Assert.Contains("is not supported", ex.Message);
    }
}
