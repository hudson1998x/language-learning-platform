using LLE.ModelGateway.Enums;

namespace LLE.ModelGateway.Models;

/// <summary>
/// Describes a chat model available through the gateway.
/// Designed for text-only conversational use cases (e.g. language learning),
/// supporting both local runtimes and remote API providers.
/// </summary>
public class ChatModelInfo
{
    /// <summary>
    /// Display name or identifier of the model (e.g. "gpt-4o-mini", "mistral-small", "llama3:8b").
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Provider or runtime source (e.g. OpenAI, Mistral, Ollama, LocalRuntime).
    /// </summary>
    public string Provider { get; set; } = string.Empty;

    /// <summary>
    /// Optional model version / tag / revision identifier.
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// Indicates whether the model is served locally or via remote API.
    /// </summary>
    public ModelBackendType BackendType { get; set; }

    /// <summary>
    /// Maximum number of tokens the model can consider in a single request (context window).
    /// </summary>
    public int ContextWindow { get; set; }

    /// <summary>
    /// Maximum tokens the model is allowed to generate per response.
    /// </summary>
    public int MaxOutputTokens { get; set; }

    /// <summary>
    /// Whether streaming responses are supported (useful for conversational UX).
    /// </summary>
    public bool SupportsStreaming { get; set; }

    /// <summary>
    /// Whether the model supports tool/function calling (likely false for most local models).
    /// </summary>
    public bool SupportsToolCalling { get; set; }

    /// <summary>
    /// Typical latency estimate in milliseconds (useful for UX pacing in learning apps).
    /// </summary>
    public int? TypicalLatencyMs { get; set; }

    /// <summary>
    /// Cost per 1K input tokens (if applicable; null for local models).
    /// </summary>
    public decimal? InputCostPer1KTokens { get; set; }

    /// <summary>
    /// Cost per 1K output tokens (if applicable; null for local models).
    /// </summary>
    public decimal? OutputCostPer1KTokens { get; set; }

    /// <summary>
    /// Free-form metadata for provider-specific configuration (model IDs, endpoints, quantization info, etc).
    /// </summary>
    public Dictionary<string, string>? Metadata { get; set; }
}