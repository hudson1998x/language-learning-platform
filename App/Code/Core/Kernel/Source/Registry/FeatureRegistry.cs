using LLE.Kernel.Events;
using Microsoft.AspNetCore.Http;

namespace LLE.Kernel.Registry;

/// <summary>
/// Entry point for registering features with the kernel. Registration is a
/// fire-and-forget operation: the supplied <see cref="Feature{TInput, TOutput}"/>
/// is compiled into a non-generic <see cref="FeatureDefinition"/> and dispatched
/// via the eventing system. <see cref="FeatureRegistry"/> holds no state itself —
/// it is purely a compile-then-dispatch step.
/// </summary>
public static class FeatureRegistry
{
    /// <summary>
    /// Compiles the given <paramref name="feature"/> into a <see cref="FeatureDefinition"/>
    /// and dispatches it through <c>FeatureEvents.Features</c>.
    /// </summary>
    /// <typeparam name="TInput">The input type accepted by the feature's handler.</typeparam>
    /// <typeparam name="TOutput">The output type produced by the feature's handler.</typeparam>
    /// <param name="feature">The feature to register.</param>
    /// <remarks>
    /// The dispatch is not awaited. Exceptions raised during dispatch (as opposed to
    /// during compilation) will not propagate to the caller.
    /// </remarks>
    public static void Add<TInput, TOutput>(Feature<TInput, TOutput> feature) where TOutput : class
    {
        var definition = FeatureCompiler.Compile(feature);

        _ = Eventing.Eventing.Of<FeatureEvents>().Features.DispatchAsync(definition);
    }
}

/// <summary>
/// Describes a single feature: the route it is reachable at, the handler that
/// implements it, and any exception-to-output mapping rules to apply if the
/// handler throws.
/// </summary>
/// <typeparam name="TInput">The input type accepted by <see cref="Handler"/>.</typeparam>
/// <typeparam name="TOutput">The output type produced by <see cref="Handler"/>.</typeparam>
public class Feature<TInput, TOutput>
{
    /// <summary>
    /// The route at which this feature is invoked (e.g. an endpoint path or command name).
    /// </summary>
    public required string Route { get; init; }

    /// <summary>
    /// The HTTP method this feature responds to (e.g. <see cref="HttpMethod.Post"/>).
    /// </summary>
    public required HttpMethod Method { get; init; }

    /// <summary>
    /// The function that implements the feature's behavior, mapping an input to an output.
    /// </summary>
    public required Func<TInput, HttpContext, ValueTask<TOutput>> Handler { get; init; }

    /// <summary>
    /// Exception-handling rules keyed by the exception <see cref="Type"/> they apply to.
    /// When <see cref="Handler"/> throws an exception of a matching type, the corresponding
    /// rule's <see cref="FeatureExceptionRule{TOutput}.Map"/> is used to produce a fallback
    /// output instead of letting the exception propagate.
    /// </summary>
    public Dictionary<Type, FeatureExceptionRule<TOutput>> Catch { get; init; } = [];
}

/// <summary>
/// Maps an exception of a specific type to a fallback output value.
/// </summary>
/// <typeparam name="TOutput">The output type produced by <see cref="Map"/>.</typeparam>
public class FeatureExceptionRule<TOutput>
{
    /// <summary>
    /// Produces a fallback output from a caught exception of type <see cref="ExceptionType"/>.
    /// </summary>
    public required Func<Exception, TOutput> Map { get; init; }
}

/// <summary>
/// A non-generic, type-erased description of a compiled <see cref="Feature{TInput, TOutput}"/>,
/// suitable for storage, dispatch, and uniform handling regardless of the feature's
/// original input/output types.
/// </summary>
/// <param name="Route">The route at which this feature is invoked.</param>
/// <param name="Method">The HTTP method this feature responds to.</param>
/// <param name="InputType">The original, erased <c>TInput</c> type of the feature.</param>
/// <param name="OutputType">The original, erased <c>TOutput</c> type of the feature.</param>
/// <param name="Executor">
/// A boxed invoker that accepts a boxed input and returns a boxed output. Internally
/// casts the input back to <c>TInput</c> and invokes the original typed handler.
/// </param>
/// <param name="ExceptionRules">
/// The feature's exception-mapping rules, type-erased to operate on boxed outputs.
/// </param>
public record FeatureDefinition(
    string Route,
    HttpMethod Method,
    Type InputType,
    Type OutputType,
    Func<object, HttpContext, ValueTask<object>> Executor,
    Dictionary<Type, FeatureExceptionRule<object>> ExceptionRules
);

/// <summary>
/// Compiles a strongly-typed <see cref="Feature{TInput, TOutput}"/> into a
/// type-erased <see cref="FeatureDefinition"/>.
/// </summary>
internal static class FeatureCompiler
{
    /// <summary>
    /// Compiles <paramref name="feature"/> into a <see cref="FeatureDefinition"/>,
    /// erasing its generic input/output types.
    /// </summary>
    /// <typeparam name="TInput">The input type accepted by the feature's handler.</typeparam>
    /// <typeparam name="TOutput">The output type produced by the feature's handler.</typeparam>
    /// <param name="feature">The feature to compile.</param>
    /// <returns>The compiled, type-erased <see cref="FeatureDefinition"/>.</returns>
    public static FeatureDefinition Compile<TInput, TOutput>(Feature<TInput, TOutput> feature) where TOutput : class
    {
        return new FeatureDefinition(
            Route: feature.Route,
            Method: feature.Method,
            InputType: typeof(TInput),
            OutputType: typeof(TOutput),
            Executor: BuildExecutor(feature.Handler),
            ExceptionRules: BuildRules(feature.Catch)
        );
    }

    /// <summary>
    /// Wraps a strongly-typed handler in a boxed-input/boxed-output delegate so it
    /// can be invoked without knowledge of its original generic types.
    /// </summary>
    /// <typeparam name="TInput">The input type accepted by <paramref name="handler"/>.</typeparam>
    /// <typeparam name="TOutput">The output type produced by <paramref name="handler"/>.</typeparam>
    /// <param name="handler">The original typed handler.</param>
    /// <returns>
    /// A delegate that casts its boxed input to <typeparamref name="TInput"/>, invokes
    /// <paramref name="handler"/>, and boxes the result.
    /// </returns>
    private static Func<object, HttpContext, ValueTask<object>> BuildExecutor<TInput, TOutput>(
        Func<TInput, HttpContext, ValueTask<TOutput>> handler)
    {
        return async (input, ctx) =>
        {
            var result = await handler((TInput)input, ctx);
            return (object)result!;
        };
    }

    /// <summary>
    /// Type-erases a feature's exception rules so they operate on boxed outputs.
    /// </summary>
    /// <remarks>
    /// <see cref="Dictionary{TKey, TValue}"/> is invariant in its value type, so a
    /// <c>Dictionary&lt;Type, FeatureExceptionRule&lt;TOutput&gt;&gt;</c> cannot be passed or cast
    /// directly where a <c>Dictionary&lt;Type, FeatureExceptionRule&lt;object&gt;&gt;</c> is expected —
    /// each entry has to be rebuilt into a new dictionary instead. The original exception-type keys are
    /// preserved, since <see cref="FeatureDefinition.ExceptionRules"/> is looked up by exception type at
    /// dispatch time.
    /// </remarks>
    /// <typeparam name="TOutput">The original output type of the rules being erased.</typeparam>
    /// <param name="rules">The typed exception rules, keyed by exception type.</param>
    /// <returns>
    /// The type-erased rules, keyed by the same exception types as <paramref name="rules"/>.
    /// </returns>
    private static Dictionary<Type, FeatureExceptionRule<object>> BuildRules<TOutput>(
        Dictionary<Type, FeatureExceptionRule<TOutput>> rules)
    {
        var erased = new Dictionary<Type, FeatureExceptionRule<object>>(rules.Count);

        foreach (var (exceptionType, rule) in rules)
        {
            erased[exceptionType] = new FeatureExceptionRule<object>
            {
                Map = ex => (object)rule.Map(ex)!
            };
        }

        return erased;
    }
}