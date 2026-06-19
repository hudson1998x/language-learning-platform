using LLE.Sockets.Enums;

namespace LLE.Sockets.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class HttpMethodAttribute(HttpVerb verb, string path) : Attribute
{
    public readonly HttpVerb Verb = verb;
    
    public readonly string Path = path;
}