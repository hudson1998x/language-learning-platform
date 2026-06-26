import { useEffect, useState } from "react";
import { listOllamaModels } from '@api/ollama';
import { register } from '@registry';

interface ModelSelectorProps {
    value: unknown;
    onChange: (value: unknown) => void;
}

const OllamaModelSelector = ({ value, onChange }: ModelSelectorProps) => {
    const [models, setModels] = useState<string[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        listOllamaModels()
            .then((res) => {
                if (res.success && res.data) {
                    setModels(res.data);
                }
            })
            .finally(() => setLoading(false));
    }, []);

    return (
        <select
            value={String(value ?? '')}
            onChange={(e) => onChange(e.target.value)}
            disabled={loading}
        >
            {loading && <option>Loading models...</option>}
            {models.map((model) => (
                <option key={model} value={model}>{model}</option>
            ))}
        </select>
    );
};

register('@config/ollama/model-selector', OllamaModelSelector);
