namespace LLE.ModelGateway.Enums;

/// <summary>
/// Describes where the model execution happens.
/// </summary>
public enum ModelBackendType : byte
{
    RemoteApi,
    LocalProcess,
    LocalServer
}