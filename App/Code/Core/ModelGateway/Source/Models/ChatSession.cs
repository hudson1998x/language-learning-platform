using LLE.ModelGateway.Enums;

namespace LLE.ModelGateway.Models;

/// <summary>
/// Represents a single conversational session containing an ordered sequence of chat messages
/// along with contextual rules that influence model behaviour.
/// </summary>
public class ChatSession
{
    /// <summary>
    /// Unique identifier for this chat session instance.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Internal storage of messages in chronological insertion order.
    /// </summary>
    private readonly List<ChatMessage> _messages = [];

    /// <summary>
    /// Read-only view of the session's messages in order of insertion.
    /// </summary>
    public IReadOnlyCollection<ChatMessage> Messages => _messages;

    /// <summary>
    /// Collection of contextual rules that shape model behaviour for this session.
    /// These are not chat messages, but persistent instructions applied to the session context.
    /// </summary>
    public readonly List<ChatContextRule> ContextRules = [];

    /// <summary>
    /// Adds an existing chat message instance to the session.
    /// </summary>
    /// <param name="message">The message to append.</param>
    public void AddMessage(ChatMessage message)
    {
        _messages.Add(message);
    }

    /// <summary>
    /// Creates and adds a new chat message with the specified content and role.
    /// The timestamp is automatically set to the current UTC time.
    /// </summary>
    /// <param name="message">The message text content.</param>
    /// <param name="role">The role of the message sender (user, assistant, system, etc.).</param>
    public void AddMessage(string message, ChatMessageRole role)
    {
        AddMessage(new ChatMessage
        {
            Message = message,
            Timestamp = DateTimeOffset.UtcNow,
            Role = role
        });
    }

    /// <summary>
    /// Removes all messages from the session, resetting conversational state.
    /// </summary>
    public void Clear()
    {
        _messages.Clear();
    }

    /// <summary>
    /// Removes a specific message instance from the session.
    /// </summary>
    /// <param name="message">The message to remove.</param>
    public void RemoveMessage(ChatMessage message)
    {
        _messages.Remove(message);
    }

    /// <summary>
    /// Removes all messages matching the specified unique identifier, if present.
    /// </summary>
    /// <param name="id">The identifier of the message to remove.</param>
    public void RemoveMessage(Guid id)
    {
        _messages.RemoveAll(x => x.Id == id);
    }
}