using LLE.Kernel.Attributes;

namespace LLE.Auth.Configurations;

[Configuration("Platform Configuration", alias: "Single user settings")]
public class SingleUserConfiguration
{
    /// <summary>
    /// Skip the login screen, this means you can log straight in as a regular user
    /// without having to re-login every time, useful for singular account instances.
    /// </summary>
    public bool IsSingleUserOnly { get; set; } = true;
}