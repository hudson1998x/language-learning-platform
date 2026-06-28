namespace LLE.Kernel.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class ConfigurationAttribute(string? groupName = null, int sortOrder = int.MaxValue, string? alias = null) : Attribute
{
    public readonly string? GroupName = groupName;
    public readonly int SortOrder = sortOrder;
    public readonly string? Alias = alias;
}