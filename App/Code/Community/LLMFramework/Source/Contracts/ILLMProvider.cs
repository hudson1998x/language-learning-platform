using LLE.LLMFramework.Models;

namespace LLE.LLMFramework.Contracts;

public interface ILLMProvider
{
    Task<LLMResponse> GenerateAsync(LLMRequest request);
}
