namespace LLE.ModelGateway.Models;

/// <summary>
/// Represents a contextual instruction that influences how the model should behave
/// during a chat session (e.g. tone, roleplay constraints, learning rules).
/// </summary>
public class ChatContextRule
{
    /// <summary>
    /// The contextual instruction or system-style message to apply to the session.
    /// Example: "Respond only in Spanish" or "Keep responses under 1 sentence".
    /// </summary>
    public string ContextMessage { get; set; } = string.Empty;

    /// <summary>
    /// Relative importance of this rule when multiple rules are present.
    /// Higher values indicate stronger precedence when resolving conflicts.
    /// </summary>
    public float Priority { get; set; } = 0.0f;
}