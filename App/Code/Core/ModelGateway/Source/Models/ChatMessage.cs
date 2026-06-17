using LLE.ModelGateway.Enums;

namespace LLE.ModelGateway.Models
{
    /// <summary>
    /// Represents a single message within a chat session, including its content,
    /// role, timestamp, and unique identifier.
    /// </summary>
    public class ChatMessage
    {
        /// <summary>
        /// The role of the message sender (e.g. user, assistant, system).
        /// </summary>
        public ChatMessageRole Role { get; set; }

        /// <summary>
        /// The textual content of the message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// The UTC timestamp indicating when the message was created.
        /// </summary>
        public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;

        /// <summary>
        /// Unique identifier for this message.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
    }
}