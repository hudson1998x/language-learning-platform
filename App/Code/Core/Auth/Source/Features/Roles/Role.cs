using LLE.Kernel.Attributes;
using LLE.Kernel.Data;

namespace LLE.Auth.Features.Roles;

[Entity]
public class Role : ContentWithId
{
    public string Key { get; set; }
    
    public string Name { get; set; }
    
    public string Description { get; set; }
}