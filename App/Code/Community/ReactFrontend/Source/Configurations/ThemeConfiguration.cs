using LLE.Kernel.Attributes;

namespace LLE.ReactFrontend.Configurations;

[Configuration]
public class ThemeConfiguration
{
    public string FrontendTheme { get; set; } = "Default";
    
    public string BackendTheme { get; set; } = "Default";
}