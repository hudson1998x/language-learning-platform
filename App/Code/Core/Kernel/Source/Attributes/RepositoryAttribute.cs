namespace LLE.Kernel.Attributes
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class RepositoryAttribute(Type entityType) : Attribute
    {
        public readonly Type EntityType = entityType;
    }
}