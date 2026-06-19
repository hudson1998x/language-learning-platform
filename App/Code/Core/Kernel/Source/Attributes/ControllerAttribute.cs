namespace LLE.Kernel.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ControllerAttribute(string baseUrl = "/") : Attribute
    {
        public readonly string BaseUrl = baseUrl;
    }
}