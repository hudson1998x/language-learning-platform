namespace LLE.Kernel.Attributes
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class RepositoryAttribute(Type entityType) : Attribute
    {
        public readonly Type EntityType = entityType;
        public bool IsCached = false;
        public int CacheSize = 0;
    }
}