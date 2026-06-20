using Microsoft.AspNetCore.Http;

namespace LLE.Sockets;

public partial class HttpSocket
{
    // allow custom serializers for certain classes 
    private readonly Dictionary<Type, Func<HttpContext, object, ValueTask>> _serializers = new();

    /// <summary>
    /// Add a custom serializer for a certain type. 
    /// </summary>
    /// <param name="serializer"></param>
    /// <typeparam name="T"></typeparam>
    public void AddSerializer<T>(Func<HttpContext, T, ValueTask> serializer) where T : class
    {
        _serializers[typeof(T)] = (ctx, obj) => serializer(ctx, (T)obj);
    }

    private async Task WriteResponse(HttpContext ctx, object? result)
    {
        if (result is null)
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        if (_serializers.TryGetValue(result.GetType(), out var serializer))
        {
            await serializer(ctx, result);
            return;
        }

        ctx.Response.ContentType = "application/json";
        await ctx.Response.WriteAsJsonAsync(result);
    }
}