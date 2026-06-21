namespace LLE.Eventing;

/// <summary>
/// Represents a single named event for a payload of type <typeparamref name="T"/>.
/// <see cref="Pipeline(Action{T})"/> handlers run sequentially and may write/transform the
/// payload, each one's output feeding the next. <see cref="Concurrent(Action{T})"/> handlers
/// run in parallel via <see cref="Task.WhenAll(Task[])"/> against a read-only snapshot — their
/// return values are discarded, for independent side effects that don't depend on each other.
/// All exceptions, from both stages, are collected and thrown together as one
/// <see cref="AggregateException"/>.
/// </summary>
/// <typeparam name="T">The type of payload dispatched through this event.</typeparam>
public sealed class EventCollection<T>
{
    private readonly List<Func<T, ValueTask<T>>> _pipeline = [];
    private readonly List<Func<T, ValueTask<T>>> _concurrent = [];
    private readonly object _gate = new();

    /// <summary>Registers a synchronous pipeline handler. Runs sequentially; may mutate the payload.</summary>
    public EventCollection<T> Pipeline(Action<T> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return Pipeline(item => { handler(item); return new ValueTask<T>(item); });
    }

    /// <summary>Registers a synchronous pipeline handler that transforms the payload into a new instance.</summary>
    public EventCollection<T> Pipeline(Func<T, T> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return Pipeline(item => new ValueTask<T>(handler(item)));
    }

    /// <summary>Registers an asynchronous pipeline handler. Runs sequentially; output feeds the next handler.</summary>
    public EventCollection<T> Pipeline(Func<T, ValueTask<T>> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        lock (_gate) _pipeline.Add(handler);
        return this;
    }

    /// <summary>Registers a synchronous concurrent handler. Read-only — its return value is discarded.</summary>
    public EventCollection<T> Concurrent(Action<T> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        return Concurrent(item => { handler(item); return new ValueTask<T>(item); });
    }

    /// <summary>Registers an asynchronous concurrent handler. Read-only — its return value is discarded.</summary>
    public EventCollection<T> Concurrent(Func<T, ValueTask<T>> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        lock (_gate) _concurrent.Add(handler);
        return this;
    }



    /// <summary>
    /// Dispatches <paramref name="item"/> through every pipeline handler in order (each one's
    /// output becomes the next input), then through every concurrent handler in parallel via
    /// <see cref="Task.WhenAll(Task[])"/> against the final pipeline result. Concurrent handler
    /// results are discarded. Every handler runs regardless of earlier failures; all exceptions
    /// are aggregated.
    /// </summary>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="item"/> is <see langword="null"/>.</exception>
    /// <exception cref="AggregateException">Thrown if one or more handlers failed.</exception>
    public async Task<T> DispatchAsync(T item)
    {
        ArgumentNullException.ThrowIfNull(item);

        Func<T, ValueTask<T>>[] pipeline, concurrent;
        lock (_gate)
        {
            pipeline = _pipeline.ToArray();
            concurrent = _concurrent.ToArray();
        }

        List<Exception> errors = [];

        foreach (var handler in pipeline)
        {
            try
            {
                item = await handler(item).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }
        }

        if (concurrent.Length > 0)
        {
            var tasks = new List<Task>();

            foreach (var handler in concurrent)
            {
                try
                {
                    tasks.Add(handler(item).AsTask());
                }
                catch (Exception ex)
                {
                    errors.Add(ex);
                }
            }

            try
            {
                await Task.WhenAll(tasks).ConfigureAwait(false);
            }
            catch
            {
                if (Task.WhenAll(tasks).Exception is { } aggregate)
                    errors.AddRange(aggregate.Flatten().InnerExceptions);
                else
                    throw;
            }
        }

        return errors.Count > 0 ? throw new AggregateException($"{errors.Count} handler(s) failed while dispatching {typeof(T).Name}.", errors) : item;
    }
}