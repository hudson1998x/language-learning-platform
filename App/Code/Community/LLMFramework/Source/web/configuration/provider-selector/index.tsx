import { useEffect, useState } from "react";
import { llmStatus } from '@api/llm';
import { register } from '@registry';

interface ProviderSelectorProps {
    value: unknown;
    onChange: (value: unknown) => void;
}

interface ProviderStatus {
    [name: string]: boolean;
}

const ProviderSelector = ({ value, onChange }: ProviderSelectorProps) => {
    const [providers, setProviders] = useState<ProviderStatus>({});
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        llmStatus()
            .then((res) => {
                if (res.success && res.data?.providers) {
                    setProviders(res.data.providers);
                }
            })
            .finally(() => setLoading(false));
    }, []);

    const providerNames = Object.keys(providers);

    const noProviders = (providerNames.length === 0 && !loading);
    
    return (
        <select
            value={String(value ?? '')}
            onChange={(e) => onChange(e.target.value)}
            disabled={loading}
        >
            {loading && <option>Loading providers...</option>}
            {noProviders && (
                <option value="">No providers available</option>
            )}
            {providerNames.map((name) => {
                const enabled = providers[name];
                return (
                    <option
                        key={name}
                        value={name}
                        disabled={!enabled}
                        style={!enabled ? { opacity: 0.5, fontStyle: 'italic' } : undefined}
                    >
                        {name} {!enabled ? '(disabled)' : ''}
                    </option>
                );
            })}
        </select>
    );
};

register('@config/llm/provider-selector', ProviderSelector);
