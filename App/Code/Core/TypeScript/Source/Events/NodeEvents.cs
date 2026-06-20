using LLE.Eventing;
using LLE.TypeScript.Builders;

namespace LLE.TypeScript.Events
{
    public class NodeEvents : EventTable
    {
        public readonly EventCollection<PackageJsonBuilder> PackageJson = new();   
    }
}