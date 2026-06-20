using LLE.Eventing;
using LLE.UiIR;

namespace LLE.Frontend.Events;

public class CanvasBuilderEvents : EventTable
{
    public readonly EventCollection<VNode> Created = new();
}