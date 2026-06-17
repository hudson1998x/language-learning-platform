namespace LLE.Dependencies.Utils;

/// <summary>
/// Provides helper methods for synchronously waiting on asynchronous operations.
///
/// This exists primarily for integration points where asynchronous execution is
/// required internally (such as event dispatching), but the calling code cannot
/// naturally participate in an async/await flow.
///
/// Prefer asynchronous code paths where possible. Blocking on tasks can reduce
/// scalability and may introduce deadlock risks if used incorrectly.
/// </summary>
internal static class AsyncUtils
{
    /// <summary>
    /// Blocks the current thread until the specified task completes and returns
    /// its result.
    ///
    /// Intended for bridging asynchronous APIs into synchronous call paths where
    /// propagating async/await is not practical.
    /// </summary>
    /// <typeparam name="T">
    /// The result type produced by the task.
    /// </typeparam>
    /// <param name="t">
    /// The task to wait for.
    /// </param>
    /// <returns>
    /// The completed task result.
    /// </returns>
    internal static T Await<T>(Task<T> t)
    {
        t.Wait();
        return t.Result;
    }
}