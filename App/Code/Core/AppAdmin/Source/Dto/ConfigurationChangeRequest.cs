using System.Text.Json;

namespace LLE.AppAdmin.Dto;

public class ConfigurationChangeRequest
{
    public string ConfigurationType { get; set; } = string.Empty;
    public JsonElement Configuration { get; set; }
}
