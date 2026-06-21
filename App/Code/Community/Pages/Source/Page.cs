using System.Text.Json;
using LLE.Kernel.Attributes;
using LLE.Kernel.Data;
using LLE.UiIR;

namespace LLE.Pages;

[Entity]
public class Page : ContentWithId
{
    public string Title { get; set; }
    
    [Unique]
    public string Key { get; set; }
    
    public string PageJson { get; set; }
    
    public string Url { get; set; }

    public void From(VNode node)
    {
        PageJson = JsonSerializer.Serialize(node);
    }
}