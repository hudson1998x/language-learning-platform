namespace LLE.AppAdmin.Dto;

public class ConfigTypeInfo
{
    public Dictionary<string, ConfigFieldInfo> Fields { get; set; } = new();
    public List<ConfigHelpInfo>? Help { get; set; }
}
