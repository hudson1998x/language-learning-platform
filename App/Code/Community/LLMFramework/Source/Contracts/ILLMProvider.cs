using LLE.LLMFramework.Models;

namespace LLE.LLMFramework.Contracts;

public interface ILLMProvider
{
    bool IsEnabled { get; }
    string? LogoUrl { get; }
    Task<LLMResponse> GenerateAsync(LLMRequest request);
}
