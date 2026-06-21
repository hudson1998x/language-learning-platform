using LLE.Kernel.Attributes;
using LLE.Kernel.Data;

namespace LLE.Languages;

[Entity]
public class Language : ContentWithId
{
    [Unique]
    public string Name { get; set; } = string.Empty;
    
    public string Description { get; set; } = string.Empty;
    
    public string FlagIcon { get; set; } = string.Empty;
}