using LLE.Kernel.Events;
using LLE.Kernel.Registry;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace LLE.Sockets;

public sealed partial class HttpSocket
{
    /// <summary>
    /// Features registered via <see cref="FeatureEvents.Features"/> before the underlying
    /// <see cref="WebApplication"/> was built. Flushed and mapped onto the app once it
    /// becomes available in <see cref="StartAsync"/>; thereafter, new registrations are
    /// mapped immediately.
    /// </summary>
    private readonly List<FeatureDefinition> _pendingFeatures = new();

    /// <summary>
    /// Subscribes to <see cref="FeatureEvents.Features"/>, mapping each <see cref="FeatureDefinition"/>
    /// onto the running <see cref="WebApplication"/> as it is dispatched. If the application has not
    /// been built yet, the definition is queued and mapped once <see cref="StartAsync"/> builds it.
    /// </summary>
    private void SubscribeToFeatures()
    {
        Eventing.Eventing.Of<FeatureEvents>().Features.Concurrent(definition =>
        {
            if (_application is { } app)
            {
                MapFeature(app, definition);
            }
            else
            {
                _pendingFeatures.Add(definition);
            }
        });
    }

    /// <summary>
    /// Maps any features that were registered before the <see cref="WebApplication"/> existed.
    /// Called once, after the app is built in <see cref="StartAsync"/>.
    /// </summary>
    /// <param name="app">The built application to map the pending features onto.</param>
    private void FlushPendingFeatures(WebApplication app)
    {
        foreach (var definition in _pendingFeatures)
        {
            MapFeature(app, definition);
        }

        _pendingFeatures.Clear();
    }

    /// <summary>
    /// Maps a single <see cref="FeatureDefinition"/> onto <paramref name="app"/> using the
    /// HTTP method specified by <see cref="FeatureDefinition.Method"/>. The request body is
    /// deserialized as JSON into <see cref="FeatureDefinition.InputType"/>, passed to the
    /// feature's executor, and the result is written via <see cref="WriteResponse"/>. If the
    /// executor throws, any matching <see cref="FeatureExceptionRule{TOutput}"/> on the
    /// definition is used to produce a fallback response instead of propagating the exception.
    /// </summary>
    /// <param name="app">The application to map the feature's route onto.</param>
    /// <param name="definition">The feature to map.</param>
    private void MapFeature(WebApplication app, FeatureDefinition definition)
    {
        app.MapMethods(definition.Route, [definition.Method.Method], async (HttpContext ctx) =>
        {
            object? input = null;

            if (definition.Method != HttpMethod.Get && definition.Method != HttpMethod.Delete)
            {
                input = await ctx.Request.ReadFromJsonAsync(definition.InputType, ctx.RequestAborted);
            }

            try
            {
                var result = await definition.Executor(input!, ctx);
                await WriteResponse(ctx, result);
            }
            catch (Exception ex)
            {
                definition.ExceptionRules.TryGetValue(ex.GetType(), out var rule);

                if (rule is null)
                {
                    throw;
                }

                var mapped = rule.Map(ex);
                await WriteResponse(ctx, mapped);
            }
        });
    }
}