using LLE.Kernel.Attributes;
using LLE.Kernel.Data;

namespace LLE.Auth.Features.Roles;

[Entity]
public class Role : ContentWithId
{
    public required string Key { get; set; }
    
    public required string Name { get; set; }
    
    public required string Description { get; set; }
}