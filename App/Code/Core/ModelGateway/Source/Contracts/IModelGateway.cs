using LLE.ModelGateway.Models;

namespace LLE.ModelGateway.Contracts;

/// <summary>
/// Defines a contract for interacting with a language model gateway.
/// Provides operations for sending chat sessions to a model and retrieving model metadata.
/// </summary>
public interface IModelGateway
{
    /// <summary>
    /// Sends a chat session to the underlying model and returns a generated response.
    /// </summary>
    /// <param name="request">
    /// The chat session containing the conversation history and context to be processed by the model.
    /// </param>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the request if it is no longer needed.
    /// </param>
    /// <returns>
    /// A <see cref="ChatSession"/> containing the model's generated reply and any associated metadata.
    /// </returns>
    Task<ChatSession> ChatAsync(ChatSession request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves information about the currently configured chat model.
    /// </summary>
    /// <param name="cancellationToken">
    /// A token that can be used to cancel the request if it is no longer needed.
    /// </param>
    /// <returns>
    /// A <see cref="ChatModelInfo"/> describing the model's capabilities, version, and configuration.
    /// </returns>
    Task<ChatModelInfo> GetModelInfoAsync(CancellationToken cancellationToken = default);
}