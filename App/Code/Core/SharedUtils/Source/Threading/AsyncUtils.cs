namespace LLE.SharedUtils.Threading;

/// <summary>
/// Provides utilities for synchronously bridging into asynchronous code.
/// </summary>
/// <remarks>
/// <para>
/// <b>WARNING — boot-phase use only.</b> Synchronously blocking on a <see cref="Task"/> is
/// an established anti-pattern: it can cause deadlocks (especially under a <c>SynchronizationContext</c>
/// that isn't free-threaded), wastes a thread-pool thread for the duration of the wait, and
/// silently wraps any exception from the task in an <see cref="AggregateException"/>.
/// </para>
/// <para>
/// This class exists solely so internal framework code can avoid making the entire application
/// stack <c>async</c> just to support a handful of synchronous extension points during startup
/// (e.g. dispatching configuration events from inside a synchronous Kestrel callback). It is
/// <b>not</b> a general-purpose async/sync bridge.
/// </para>
/// <para>
/// Do not call this from request-handling code, hot paths, or anywhere after the application
/// has finished booting. If you find yourself reaching for this outside of internal startup
/// plumbing, that's a sign the surrounding API should be made <c>async</c> instead.
/// </para>
/// </remarks>
public static class AsyncUtils
{
    /// <summary>
    /// Synchronously blocks the calling thread until the given task completes, then returns
    /// its result.
    /// </summary>
    /// <remarks>
    /// See the <see cref="AsyncUtils"/> class-level remarks — this is boot-phase-only,
    /// internals-only plumbing and should not be used elsewhere.
    /// </remarks>
    /// <param name="task">The task to wait on and unwrap the result of.</param>
    /// <typeparam name="T">The type of the task's result.</typeparam>
    /// <returns>The result of <paramref name="task"/> once it has completed.</returns>
    public static T Await<T>(Task<T> task)
    {
        task.Wait();
        return task.Result;
    }
}