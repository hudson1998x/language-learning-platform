namespace LLE.AppAdmin.Dto;

public class ConfigTypeInfo
{
    public Dictionary<string, ConfigFieldInfo> Fields { get; set; } = new();
    public List<ConfigHelpInfo>? Help { get; set; }
    public string? GroupName { get; set; }
    public int SortOrder { get; set; } = int.MaxValue;
    public string? Alias { get; set; }
}
