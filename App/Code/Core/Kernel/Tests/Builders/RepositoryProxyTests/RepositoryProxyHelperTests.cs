using LLE.Kernel.Builders;
using LLE.Kernel.Contracts;
using LLE.Kernel.DataQL.Ast;
using LLE.Kernel.Security;
using Tests.Builders.RepositoryProxyTests.Fakes;

namespace Tests.Builders.RepositoryProxyTests;

public sealed class RepositoryProxyHelperTests
{
    // ─── FindInCache ─────────────────────────────────────────────────────

    [Fact]
    public void FindInCache_ReturnsEntity_WhenFound()
    {
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Target" };
        var cache = new object[] { new TestEntity { Id = Guid.NewGuid() }, entity, null! };

        var result = RepositoryProxyHelper.FindInCache(cache, typeof(TestEntity), entity.Id);

        Assert.Same(entity, result);
    }

    [Fact]
    public void FindInCache_ReturnsNull_WhenNotFound()
    {
        var cache = new object[] { new TestEntity { Id = Guid.NewGuid() } };

        var result = RepositoryProxyHelper.FindInCache(cache, typeof(TestEntity), Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public void FindInCache_ReturnsNull_ForEmptySlots()
    {
        var cache = new object[] { null!, null! };

        var result = RepositoryProxyHelper.FindInCache(cache, typeof(TestEntity), Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public void FindInCache_SkipsEntitiesOfDifferentType()
    {
        var id = Guid.NewGuid();
        var cache = new object[] { new TestEntityWithoutId { Name = "Wrong" } };

        var result = RepositoryProxyHelper.FindInCache(cache, typeof(TestEntity), id);

        Assert.Null(result);
    }

    [Fact]
    public void FindInCache_ReturnsNull_WhenEntityTypeHasNoIdProperty()
    {
        var cache = new object[] { new TestEntityWithoutId { Name = "Test" } };

        var result = RepositoryProxyHelper.FindInCache(cache, typeof(TestEntityWithoutId), Guid.NewGuid());

        Assert.Null(result);
    }

    // ─── FindAllFromCache ────────────────────────────────────────────────

    [Fact]
    public void FindAllFromCache_ReturnsAllEntitiesOfType()
    {
        var entity1 = new TestEntity { Name = "A" };
        var entity2 = new TestEntity { Name = "B" };
        var cache = new object[] { entity1, null!, entity2, null! };

        var results = RepositoryProxyHelper.FindAllFromCache<TestEntity>(cache);

        Assert.Equal(2, results.Count);
        Assert.Contains(entity1, results);
        Assert.Contains(entity2, results);
    }

    [Fact]
    public void FindAllFromCache_ReturnsEmptyList_WhenNoMatchingEntities()
    {
        var cache = new object[] { null!, null! };

        var results = RepositoryProxyHelper.FindAllFromCache<TestEntity>(cache);

        Assert.Empty(results);
    }

    [Fact]
    public void FindAllFromCache_SkipsNullAndWrongTypes()
    {
        var entity = new TestEntity { Name = "Valid" };
        var cache = new object[] { entity, "string", 42, null! };

        var results = RepositoryProxyHelper.FindAllFromCache<TestEntity>(cache);

        Assert.Single(results);
        Assert.Same(entity, results[0]);
    }

    // ─── InsertIntoCache ─────────────────────────────────────────────────

    [Fact]
    public void InsertIntoCache_InsertsIntoFirstNullSlot()
    {
        var cache = new object[] { new TestEntity(), null!, null! };
        var item = new TestEntity { Name = "Inserted" };

        RepositoryProxyHelper.InsertIntoCache(cache, item);

        Assert.Same(item, cache[1]);
    }

    [Fact]
    public void InsertIntoCache_DoesNotOverwriteExistingEntries()
    {
        var existing = new TestEntity { Name = "Existing" };
        var cache = new object[] { existing, null! };
        var item = new TestEntity { Name = "Inserted" };

        RepositoryProxyHelper.InsertIntoCache(cache, item);

        Assert.Same(existing, cache[0]);
        Assert.Same(item, cache[1]);
    }

    [Fact]
    public void InsertIntoCache_IsNoOp_WhenCacheIsFull()
    {
        var cache = new object[] { new TestEntity(), new TestEntity() };
        var item = new TestEntity { Name = "Overflow" };

        RepositoryProxyHelper.InsertIntoCache(cache, item);

        Assert.DoesNotContain(item, cache);
    }

    // ─── UpdateInCache ───────────────────────────────────────────────────

    [Fact]
    public void UpdateInCache_UpdatesMatchingEntityById()
    {
        var id = Guid.NewGuid();
        var existing = new TestEntity { Id = id, Name = "Old" };
        var cache = new object[] { existing, null! };
        var updated = new TestEntity { Id = id, Name = "Updated" };

        RepositoryProxyHelper.UpdateInCache(cache, updated, typeof(TestEntity));

        Assert.Same(updated, cache[0]);
    }

    [Fact]
    public void UpdateInCache_InsertsWhenIdNotFound()
    {
        var cache = new object[] { new TestEntity { Id = Guid.NewGuid() }, null! };
        var item = new TestEntity { Id = Guid.NewGuid(), Name = "New" };

        RepositoryProxyHelper.UpdateInCache(cache, item, typeof(TestEntity));

        Assert.Same(item, cache[1]);
    }

    [Fact]
    public void UpdateInCache_IsNoOp_WhenEntityTypeHasNoIdProperty()
    {
        var cache = new object[] { new TestEntityWithoutId { Name = "Old" }, null! };
        var item = new TestEntityWithoutId { Name = "New" };

        RepositoryProxyHelper.UpdateInCache(cache, item, typeof(TestEntityWithoutId));

        // No crash; cache unchanged
    }

    [Fact]
    public void UpdateInCache_IsNoOp_WhenItemIdIsNull()
    {
        var cache = new object[] { new object(), null! };
        var item = new object();

        // object has no Id property → no-op, no crash
        RepositoryProxyHelper.UpdateInCache(cache, item, typeof(object));
    }

    // ─── RemoveFromCache ─────────────────────────────────────────────────

    [Fact]
    public void RemoveFromCache_RemovesMatchingEntityById()
    {
        var id = Guid.NewGuid();
        var entity = new TestEntity { Id = id, Name = "RemoveMe" };
        var cache = new object[] { entity, new TestEntity { Id = Guid.NewGuid() } };

        RepositoryProxyHelper.RemoveFromCache(cache, entity, typeof(TestEntity));

        Assert.Null(cache[0]);
        Assert.NotNull(cache[1]);
    }

    [Fact]
    public void RemoveFromCache_IsNoOp_WhenEntityNotInCache()
    {
        var cache = new object[] { new TestEntity { Id = Guid.NewGuid() }, null! };
        var item = new TestEntity { Id = Guid.NewGuid() };

        RepositoryProxyHelper.RemoveFromCache(cache, item, typeof(TestEntity));

        Assert.NotNull(cache[0]);
    }

    [Fact]
    public void RemoveFromCache_IsNoOp_WhenEntityTypeHasNoIdProperty()
    {
        var cache = new object[] { new TestEntityWithoutId { Name = "Keep" } };
        var item = new TestEntityWithoutId { Name = "Remove" };

        RepositoryProxyHelper.RemoveFromCache(cache, item, typeof(TestEntityWithoutId));

        // No crash; cache unchanged
    }

    [Fact]
    public void RemoveFromCache_IsNoOp_WhenItemIdIsNull()
    {
        var cache = new object[] { new object(), null! };
        var item = new object();

        RepositoryProxyHelper.RemoveFromCache(cache, item, typeof(object));
    }

    // ─── GetOrInitializeCache ────────────────────────────────────────────

    [Fact]
    public void GetOrInitializeCache_InitializesFromAdapter()
    {
        var adapter = new FakeDatabaseAdapter();
        var entity1 = new TestEntity { Id = Guid.NewGuid(), Name = "A" };
        var entity2 = new TestEntity { Id = Guid.NewGuid(), Name = "B" };
        adapter.OnExecuteQuery = _ => Task.FromResult<object>(new List<TestEntity> { entity1, entity2 });

        object[]? cache = null;
        var result = RepositoryProxyHelper.GetOrInitializeCache(ref cache, adapter, typeof(TestEntity), 10);

        Assert.NotNull(cache);
        Assert.Same(cache, result);
        Assert.Same(entity1, cache[0]);
        Assert.Same(entity2, cache[1]);
        Assert.Null(cache[2]);
        Assert.Single(adapter.ReceivedNodes);
    }

    [Fact]
    public void GetOrInitializeCache_ReturnsExistingCache_OnSubsequentCall()
    {
        var adapter = new FakeDatabaseAdapter();
        var entity = new TestEntity { Id = Guid.NewGuid() };
        adapter.OnExecuteQuery = _ => Task.FromResult<object>(new List<TestEntity> { entity });

        object[]? cache = null;
        var first = RepositoryProxyHelper.GetOrInitializeCache(ref cache, adapter, typeof(TestEntity), 5);
        var second = RepositoryProxyHelper.GetOrInitializeCache(ref cache, adapter, typeof(TestEntity), 5);

        Assert.Same(first, second);
        Assert.Single(adapter.ReceivedNodes);
    }

    [Fact]
    public void GetOrInitializeCache_HandlesNullAdapterResult()
    {
        var adapter = new FakeDatabaseAdapter();
        adapter.OnExecuteQuery = _ => Task.FromResult<object>(null!);

        object[]? cache = null;
        var result = RepositoryProxyHelper.GetOrInitializeCache(ref cache, adapter, typeof(TestEntity), 3);

        Assert.NotNull(result);
        Assert.Equal(3, result.Length);
        Assert.All(result, Assert.Null);
    }

    [Fact]
    public void GetOrInitializeCache_RespectsCacheSizeLimit()
    {
        var adapter = new FakeDatabaseAdapter();
        var items = Enumerable.Range(0, 20).Select(i => new TestEntity { Name = $"Item{i}" }).ToList();
        adapter.OnExecuteQuery = _ => Task.FromResult<object>(items);

        object[]? cache = null;
        var result = RepositoryProxyHelper.GetOrInitializeCache(ref cache, adapter, typeof(TestEntity), 5);

        Assert.Equal(5, result.Length);
        Assert.Same(items[0], result[0]);
        Assert.Same(items[1], result[1]);
        Assert.Same(items[2], result[2]);
        Assert.Same(items[3], result[3]);
        Assert.Same(items[4], result[4]);
    }

    // ─── GetOrInitializeAdapter (via proxy, bypassing event dispatch) ────

    [Fact]
    public void GetOrInitializeAdapter_ReturnsCachedAdapter_WhenAlreadySet()
    {
        var adapter = new FakeDatabaseAdapter();
        IDatabaseAdapter field = adapter;

        var result = RepositoryProxyHelper.GetOrInitializeAdapter(
            ref field, typeof(ITestRepository), typeof(TestEntity));

        Assert.Same(adapter, result);
    }

    // ─── ExecuteAsync (with bypass to avoid policy enforcement) ──────────

    [Fact]
    public async Task ExecuteAsync_WithCreateNode_PassesThroughAdapter()
    {
        var adapter = new FakeDatabaseAdapter();
        var entity = new TestEntity { Id = Guid.NewGuid(), Name = "Created" };
        adapter.OnExecuteQuery = _ => Task.FromResult<object>(entity);

        var node = new WriteQueryNode
        {
            TableName = "TestEntity",
            Payload = entity,
            EntityType = typeof(TestEntity)
        };

        var result = await RepositoryProxyHelper.ExecuteAsync<TestEntity>(
            adapter, node, UserContext.Guest, DataOptions.Bypass);

        Assert.Same(entity, result);
        Assert.Single(adapter.ReceivedNodes);
    }

    [Fact]
    public async Task ExecuteAsync_WithReadNode_PassesThroughAdapter()
    {
        var adapter = new FakeDatabaseAdapter();
        var entity = new TestEntity { Id = Guid.NewGuid() };
        adapter.OnExecuteQuery = _ => Task.FromResult<object>(entity);

        var node = new ReadQueryNode
        {
            TableName = "TestEntity",
            Where = new FilterNode { ColumnName = "Id", Operator = FilterOperator.Equals, Value = entity.Id },
            EntityType = typeof(TestEntity)
        };

        var result = await RepositoryProxyHelper.ExecuteAsync<TestEntity>(
            adapter, node, UserContext.Guest, DataOptions.Bypass);

        Assert.Same(entity, result);
        Assert.Single(adapter.ReceivedNodes);
    }

    [Fact]
    public async Task ExecuteAsync_WithListResult_CopiesToList()
    {
        var adapter = new FakeDatabaseAdapter();
        var items = new List<TestEntity>
        {
            new() { Name = "A" },
            new() { Name = "B" }
        };
        adapter.OnExecuteQuery = _ => Task.FromResult<object>(items);

        var node = new ReadQueryNode { TableName = "TestEntity", EntityType = typeof(TestEntity) };

        var result = await RepositoryProxyHelper.ExecuteAsync<List<TestEntity>>(
            adapter, node, UserContext.Guest, DataOptions.Bypass);

        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[0].Name);
        Assert.Equal("B", result[1].Name);
    }

    [Fact]
    public async Task ExecuteAsync_WithDeleteNode_PassesThroughAdapter()
    {
        var adapter = new FakeDatabaseAdapter();
        var entity = new TestEntity { Id = Guid.NewGuid() };
        adapter.OnExecuteQuery = _ => Task.FromResult<object>(entity);

        var node = new DeleteQueryNode
        {
            TableName = "TestEntity",
            Payload = entity,
            Where = new FilterNode { ColumnName = "Id", Operator = FilterOperator.Equals, Value = entity.Id },
            EntityType = typeof(TestEntity)
        };

        var result = await RepositoryProxyHelper.ExecuteAsync<TestEntity>(
            adapter, node, UserContext.Guest, DataOptions.Bypass);

        Assert.Same(entity, result);
        Assert.Single(adapter.ReceivedNodes);
    }

    // ─── ExecuteWithCacheOp ──────────────────────────────────────────────

    [Fact]
    public async Task ExecuteWithCacheOp_Insert_InsertsIntoCache()
    {
        var adapter = new FakeDatabaseAdapter();
        var entity = new TestEntity { Id = Guid.NewGuid() };
        adapter.OnExecuteQuery = _ => Task.FromResult<object>(entity);

        var node = new WriteQueryNode
        {
            TableName = "TestEntity",
            Payload = entity,
            EntityType = typeof(TestEntity)
        };
        var cache = new object[] { null!, null! };

        var result = await RepositoryProxyHelper.ExecuteWithCacheOp<TestEntity>(
            adapter, node, UserContext.Guest, DataOptions.Bypass,
            cache, typeof(TestEntity), CacheOp.Insert);

        Assert.Same(entity, result);
        Assert.Same(entity, cache[0]);
    }

    [Fact]
    public async Task ExecuteWithCacheOp_Update_UpdatesInCache()
    {
        var adapter = new FakeDatabaseAdapter();
        var id = Guid.NewGuid();
        var existing = new TestEntity { Id = id, Name = "Old" };
        var updated = new TestEntity { Id = id, Name = "Updated" };
        adapter.OnExecuteQuery = _ => Task.FromResult<object>(updated);

        var node = new WriteQueryNode
        {
            TableName = "TestEntity",
            Payload = updated,
            Where = new FilterNode { ColumnName = "Id", Operator = FilterOperator.Equals, Value = id },
            EntityType = typeof(TestEntity)
        };
        var cache = new object[] { existing, null! };

        var result = await RepositoryProxyHelper.ExecuteWithCacheOp<TestEntity>(
            adapter, node, UserContext.Guest, DataOptions.Bypass,
            cache, typeof(TestEntity), CacheOp.Update);

        Assert.Same(updated, result);
        Assert.Same(updated, cache[0]);
    }

    [Fact]
    public async Task ExecuteWithCacheOp_Remove_RemovesFromCache()
    {
        var adapter = new FakeDatabaseAdapter();
        var entity = new TestEntity { Id = Guid.NewGuid() };
        adapter.OnExecuteQuery = _ => Task.FromResult<object>(entity);

        var node = new DeleteQueryNode
        {
            TableName = "TestEntity",
            Payload = entity,
            Where = new FilterNode { ColumnName = "Id", Operator = FilterOperator.Equals, Value = entity.Id },
            EntityType = typeof(TestEntity)
        };
        var cache = new object[] { entity, null! };

        var result = await RepositoryProxyHelper.ExecuteWithCacheOp<TestEntity>(
            adapter, node, UserContext.Guest, DataOptions.Bypass,
            cache, typeof(TestEntity), CacheOp.Remove);

        Assert.Same(entity, result);
        Assert.Null(cache[0]);
    }
}
