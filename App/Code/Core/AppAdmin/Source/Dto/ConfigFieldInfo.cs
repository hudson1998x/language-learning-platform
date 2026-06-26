namespace LLE.AppAdmin.Dto;

public class ConfigFieldInfo
{
    public string Type { get; set; } = string.Empty;
    public object? Value { get; set; }
    public string? Component { get; set; }
    public List<ConfigHelpInfo>? Help { get; set; }
}
