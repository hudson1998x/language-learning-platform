namespace LLE.Kernel.Contracts;

/// <summary>
/// Defines a lifecycle contract for a modular plugin/module system.
///
/// This interface represents a module that can participate in both
/// installation (structural presence within the host system) and
/// application runtime lifecycle (startup and shutdown).
///
/// The lifecycle is intentionally split into two phases:
/// - Installation phase: concerns registration and structural setup
/// - Application phase: concerns runtime execution within the host process
/// </summary>
public interface IModuleLoader
{
    /// <summary>
    /// Called when the host application is starting.
    /// This is part of the runtime lifecycle and should be used to
    /// initialize resources required during normal execution,
    /// such as middleware, services, or runtime handlers.
    /// </summary>
    /// <returns>A task representing the asynchronous startup operation.</returns>
    public Task AppStart();

    /// <summary>
    /// Called when the host application is stopping.
    /// This is part of the runtime lifecycle and should be used to
    /// gracefully release runtime resources, complete in-flight work,
    /// and shut down any active operations.
    /// </summary>
    /// <returns>A task representing the asynchronous shutdown operation.</returns>
    public Task AppStop();

    /// <summary>
    /// Called when the module is installed into the host system.
    /// This represents the structural lifecycle phase where the module
    /// can register itself, declare services, or participate in system composition
    /// before the application runtime begins.
    /// </summary>
    /// <returns>A task representing the asynchronous installation operation.</returns>
    public Task Install();

    /// <summary>
    /// Called when the module is removed from the host system.
    /// This represents the structural lifecycle teardown phase where the module
    /// should clean up any persistent registrations, configuration, or external state
    /// that was established during installation.
    /// </summary>
    /// <returns>A task representing the asynchronous uninstallation operation.</returns>
    public Task Uninstall();
}