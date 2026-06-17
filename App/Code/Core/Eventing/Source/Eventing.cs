using System.Collections.Concurrent;

namespace LLE.Eventing;

/// <summary>
/// Provides a global registry of <see cref="EventTable"/> singletons, keyed by their concrete type.
/// Each distinct <see cref="EventTable"/> subtype is lazily created once and cached for the
/// lifetime of the application.
/// </summary>
/// <example>
/// <code>
/// Eventing.Of&lt;UserEvents&gt;().Created.DispatchAsync(...);
/// </code>
/// </example>
public static class Eventing
{
    /// <summary>
    /// Backing store mapping each registered <see cref="EventTable"/> subtype to its singleton
    /// instance. Thread-safe for concurrent reads and writes.
    /// </summary>
    private static readonly ConcurrentDictionary<Type, EventTable> Tables = [];

    /// <summary>
    /// Gets the singleton <see cref="EventTable"/> instance for the specified concrete
    /// <paramref name="type"/>, creating it via its parameterless constructor if one does
    /// not already exist.
    /// </summary>
    /// <param name="type">The concrete <see cref="EventTable"/> subtype to retrieve or create.</param>
    /// <returns>The existing or newly created instance of <paramref name="type"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if an instance of <paramref name="type"/> could not be created — for example, if it
    /// has no accessible public parameterless constructor, or does not derive from <see cref="EventTable"/>.
    /// </exception>
    public static EventTable Of(Type type) =>
        Tables.GetOrAdd(type, static t =>
            Activator.CreateInstance(t) as EventTable
            ?? throw new InvalidOperationException(
                $"Failed to create an instance of '{t}'. Ensure it has a public parameterless constructor and derives from {nameof(EventTable)}."));

    /// <summary>
    /// Gets the singleton instance of the specified <see cref="EventTable"/> subtype
    /// <typeparamref name="T"/>, creating it if one does not already exist.
    /// </summary>
    /// <typeparam name="T">The concrete <see cref="EventTable"/> subtype to retrieve or create.</typeparam>
    /// <returns>The existing or newly created instance of <typeparamref name="T"/>.</returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown if an instance of <typeparamref name="T"/> could not be created.
    /// </exception>
    /// <example>
    /// <code>
    /// Eventing.Of&lt;UserEvents&gt;().Created.DispatchAsync(...);
    /// </code>
    /// </example>
    public static T Of<T>() where T : EventTable => (T)Of(typeof(T));
}