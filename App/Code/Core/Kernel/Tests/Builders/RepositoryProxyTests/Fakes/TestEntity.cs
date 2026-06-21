namespace Tests.Builders.RepositoryProxyTests.Fakes;

public sealed class TestEntity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}

public sealed class TestEntityWithoutId
{
    public string Name { get; set; } = string.Empty;
}
