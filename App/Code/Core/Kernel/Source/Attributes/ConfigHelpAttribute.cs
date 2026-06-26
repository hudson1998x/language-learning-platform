namespace LLE.Kernel.Attributes;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
public class ConfigHelpAttribute(string component, string? tabName = null) : Attribute
{
    public readonly string Component = component;
    public readonly string? TabName = tabName;
}