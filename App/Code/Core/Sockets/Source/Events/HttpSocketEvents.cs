using LLE.Eventing;

namespace LLE.Sockets.Events;

public class HttpSocketEvents : EventTable
{
    public readonly EventCollection<HttpSocket> Ready = new();
}