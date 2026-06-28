import { useEffect, useState } from "react";
import { llmStatus } from '@api/llm';
import './style.scss';

interface ProviderInfo {
    name: string;
    enabled: boolean;
    logoUrl: string | null;
}

const PROVIDER_TAB: Record<string, string> = {
    ChatGPT: "ChatGPT",
    MistralChat: "Mistral",
    Ollama: "Ollama",
};

export const LlmNeededWall = () => {
    const [providers, setProviders] = useState<ProviderInfo[]>([]);

    useEffect(() => {
        llmStatus().then((res) => {
            if (res.success && res.data?.providers) {
                const entries = Object.entries(res.data.providers).map(([name, enabled]) => ({
                    name,
                    enabled,
                    logoUrl: res.data?.providerLogos?.[name] ?? null,
                }));
                setProviders(entries);
            }
        });
    }, []);

    return (
        <div className="llm-needed">
            <div className="llm-needed__content">
                <h1 className="llm-needed__title">AI connection required</h1>
                <p className="llm-needed__subtitle">
                    You haven't configured an LLM — you won't be able to use all the great features of the platform without one.
                </p>
                <div className="llm-needed__grid">
                    {providers.map((provider) => {
                        const tab = PROVIDER_TAB[provider.name] ?? provider.name;
                        return (
                            <a
                                key={provider.name}
                                href={`/settings?tab=${tab}`}
                                className="llm-needed__card"
                            >
                                {provider.logoUrl && (
                                    <img
                                        className="llm-needed__card-logo"
                                        src={provider.logoUrl}
                                        alt={`${provider.name} logo`}
                                    />
                                )}
                                <span className="llm-needed__card-name">{provider.name}</span>
                            </a>
                        );
                    })}
                </div>
            </div>
        </div>
    );
};
