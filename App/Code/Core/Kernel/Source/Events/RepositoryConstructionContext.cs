using LLE.Kernel.Contracts;

namespace LLE.Kernel.Events;

public class RepositoryConstructionContext
{
    public required Type RepositoryType { get; init; }
    public required Type EntityType { get; init; }
    public IDatabaseAdapter? Adapter { get; set; }
}
