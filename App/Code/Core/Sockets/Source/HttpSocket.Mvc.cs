using System.Reflection;
using System.Text.RegularExpressions;
using LLE.Kernel.Attributes;
using LLE.Sockets.Attributes;
using LLE.Sockets.Enums;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace LLE.Sockets;

public partial class HttpSocket
{
    // Controllers are owned externally and injected once
    private readonly Dictionary<Type, object> _controllers = new();
    
    // allow custom serializers for certain classes 
    private Dictionary<Type, Func<HttpContext, object, ValueTask>> _serializers = new();
    
    public void LoadControllers(object[] controllers)
    {
        foreach (var controller in controllers)
        {
            var type = controller.GetType();

            // store instance once
            _controllers[type] = controller;

            var controllerAttribute = type.GetCustomAttribute<ControllerAttribute>();
            var baseUrl = controllerAttribute?.BaseUrl ?? "/";

            var methods = type.GetMethods(
                BindingFlags.Public |
                BindingFlags.Instance |
                BindingFlags.DeclaredOnly);

            foreach (var method in methods)
            {
                var httpMethod = method.GetCustomAttribute<HttpMethodAttribute>();
                if (httpMethod is null)
                    continue;

                AddRoute(httpMethod, baseUrl, type, method);
            }
        }
    }

    private void AddRoute(
        HttpMethodAttribute httpMethod,
        string baseUrl,
        Type controllerType,
        MethodInfo method)
    {
        var fullUrl = NormalizeRoute($"{baseUrl}/{httpMethod.Path}");

        RequestDelegate handler = ctx =>
            HandleRequest(controllerType, method, ctx);

        switch (httpMethod.Verb)
        {
            case HttpVerb.Get:
                _application!.MapGet(fullUrl, handler);
                break;

            case HttpVerb.Post:
                _application!.MapPost(fullUrl, handler);
                break;

            case HttpVerb.Put:
                _application!.MapPut(fullUrl, handler);
                break;

            case HttpVerb.Delete:
                _application!.MapDelete(fullUrl, handler);
                break;

            case HttpVerb.Patch:
                _application!.MapMethods(fullUrl, ["PATCH"], handler);
                break;

            case HttpVerb.Head:
                _application!.MapMethods(fullUrl, ["HEAD"], handler);
                break;

            case HttpVerb.Options:
                _application!.MapMethods(fullUrl, ["OPTIONS"], handler);
                break;

            case HttpVerb.Trace:
                _application!.MapMethods(fullUrl, ["TRACE"], handler);
                break;

            case HttpVerb.Connect:
                _application!.MapMethods(fullUrl, ["CONNECT"], handler);
                break;

            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private static string NormalizeRoute(string route)
        => Regex.Replace(route, @":(\w+)", "{$1}");

    private async Task HandleRequest(
        Type controllerType,
        MethodInfo methodInfo,
        HttpContext ctx)
    {
        var routeValues = ctx.Request.RouteValues;
        var parameters = methodInfo.GetParameters();

        var args = new object?[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var p = parameters[i];

            if (p.ParameterType == typeof(HttpContext))
            {
                args[i] = ctx;
                continue;
            }

            if (routeValues.TryGetValue(p.Name ?? string.Empty, out var raw))
            {
                args[i] = ConvertValue(raw, p.ParameterType);
                continue;
            }

            args[i] = GetDefault(p.ParameterType);
        }

        var controller = _controllers[controllerType];

        var result = methodInfo.Invoke(controller, args);

        if (result is Task task)
        {
            await task.ConfigureAwait(false);

            var resultProperty = task.GetType().GetProperty("Result");
            var value = resultProperty?.GetValue(task);

            await WriteResponse(ctx, value);
            return;
        }

        await WriteResponse(ctx, result);
    }

    private static object? ConvertValue(object? raw, Type targetType)
    {
        if (raw is null)
            return GetDefault(targetType);

        if (targetType.IsInstanceOfType(raw))
            return raw;

        var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

        return Convert.ChangeType(raw, underlying);
    }

    private static object? GetDefault(Type type)
        => type.IsValueType ? Activator.CreateInstance(type) : null;

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