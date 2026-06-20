using System.Text;
using Microsoft.Extensions.ObjectPool;

namespace LLE.Kernel.Memory;

public static class Pooling
{
    public static readonly ObjectPool<StringBuilder> StringBuilders = new DefaultObjectPool<StringBuilder>(
        new StringBuilderPooledObjectPolicy()
    );
}