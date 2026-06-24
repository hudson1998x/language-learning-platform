using LLE.Kernel.Attributes;
using LLE.Kernel.Data;

namespace LLE.Scenarios;

[Entity]
public class Scenario : ContentWithId
{
    [Unique]
    public string Title { get; set; }
    
    public string Steps { get; set; }

    public void SetSteps(params string[] steps)
    {
        Steps = string.Join("\n", steps);
    }
}