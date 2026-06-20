namespace LLE.Kernel.DataQL.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class QueryAttribute(string query) : Attribute
    {
        public readonly string Query = query;
    }
}