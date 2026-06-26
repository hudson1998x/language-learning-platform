import { useEffect, useState } from "react";
import { listMistralModels } from '@api/mistral';
import { register } from '@registry';

interface ModelSelectorProps {
    value: unknown;
    onChange: (value: unknown) => void;
}

const MistralModelSelector = ({ value, onChange }: ModelSelectorProps) => {
    const [models, setModels] = useState<string[]>([]);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        listMistralModels()
            .then((res) => {
                if (res.success && res.data) {
                    setModels([...new Set(res.data)]);
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

register('@config/mistral/model-selector', MistralModelSelector);