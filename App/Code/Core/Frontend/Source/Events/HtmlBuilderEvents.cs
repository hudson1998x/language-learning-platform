using System.Text;
using LLE.Eventing;
using LLE.Frontend.Builders;
using LLE.Frontend.Writers;

namespace LLE.Frontend.Events;

public class HtmlBuilderEvents : EventTable
{
    public readonly EventCollection<HtmlBuilder> Created = new();
    
    public readonly EventCollection<HtmlSink> Head = new();
    
    public readonly EventCollection<HtmlSink> Body = new();
}