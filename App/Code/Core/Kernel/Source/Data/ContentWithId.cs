namespace LLE.Kernel.Data;

public class ContentWithId
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    
    public DateTime UpdateTime { get; set; } = DateTime.UtcNow;
}