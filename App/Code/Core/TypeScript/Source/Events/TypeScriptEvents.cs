using LLE.Eventing;
using LLE.TypeScript.Builders;

namespace LLE.TypeScript.Events;

public class TypeScriptEvents : EventTable
{
    public readonly EventCollection<TsConfigBuilder> TsConfig = new();
}