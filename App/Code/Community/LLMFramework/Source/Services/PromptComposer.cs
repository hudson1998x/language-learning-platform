using LLE.LLMFramework.Models;

namespace LLE.LLMFramework.Services;

public class PromptComposer
{
    public string Compose(LLMRequest request)
    {
        var sections = new List<string>();

        if (request.Instructions.Count > 0)
        {
            sections.Add("# Instructions");
            foreach (var instruction in request.Instructions)
            {
                sections.Add(instruction.Content);
            }
        }

        if (request.Context.Values.Count > 0)
        {
            sections.Add("# Context");
            foreach (var (key, value) in request.Context.Values)
            {
                sections.Add($"{key}: {value}");
            }
        }

        if (request.Variables.Values.Count > 0)
        {
            sections.Add("# Variables");
            foreach (var (key, value) in request.Variables.Values)
            {
                sections.Add($"{key}: {value}");
            }
        }

        if (request.History.Count > 0)
        {
            sections.Add("# Conversation");
            foreach (var message in request.History)
            {
                sections.Add($"{message.Role}: {message.Content}");
            }
        }

        sections.Add("# User Prompt");
        sections.Add(request.Prompt);

        return string.Join("\n\n", sections);
    }
}
