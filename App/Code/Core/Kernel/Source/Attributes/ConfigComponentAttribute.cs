namespace LLE.Kernel.Attributes;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ConfigComponentAttribute(string component) : Attribute
{
    public string Component = component;
}