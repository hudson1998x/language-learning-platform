using System.Collections.Concurrent;
using System.Reflection;
using LLE.Eventing;

namespace LLE.Sockets.Events;

public class DependencyInjectionEvents : EventTable
{
    public readonly DependencyInjectionResolve Parameter = new();
}

public class DependencyInjectionResolve
{
    private readonly ConcurrentBag<Func<ParameterInfo, object?>> _resolvers = [];
    
    public void AddResolver(Func<ParameterInfo,object> resolver)
    {
        _resolvers.Add(resolver);
    }

    public object? Resolve(ParameterInfo parameter)
    {
        foreach (var resolver in _resolvers)
        {
            var result = resolver(parameter);

            if (result != null)
            {
                return result;
            }
        }

        return null;
    }
}