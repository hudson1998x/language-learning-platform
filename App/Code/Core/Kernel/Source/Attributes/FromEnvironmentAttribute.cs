namespace LLE.Kernel.Attributes;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
public class FromEnvironmentAttribute<T>(string key, T defaultValue) : Attribute
{
    public readonly string Key = key;
    
    public readonly T DefaultValue = defaultValue;
}