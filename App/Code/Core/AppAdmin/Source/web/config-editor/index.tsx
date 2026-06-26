import { useEffect, useState } from "react";
import { listConfigs, changeSettings } from '@api/admin';
import { register } from "@registry";
import './style.scss';

const ConfigEditor = () => {
    const [configs, setConfigs] = useState<Record<string, unknown>>({});
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [selectedConfig, setSelectedConfig] = useState<string | null>(null);
    const [formValues, setFormValues] = useState<Record<string, string | number | boolean>>({});
    const [saving, setSaving] = useState(false);
    const [saveMessage, setSaveMessage] = useState<{ type: 'success' | 'error'; text: string } | null>(null);
    const [expandedModules, setExpandedModules] = useState<Set<string>>(new Set());

    useEffect(() => {
        listConfigs()
            .then((res) => {
                if (res.success && res.data) {
                    setConfigs(res.data);
                    const keys = Object.keys(res.data);
                    if (keys.length > 0) {
                        const firstKey = keys[0];
                        setSelectedConfig(firstKey);
                        setFormValues(res.data[firstKey] as Record<string, string | number | boolean>);
                        const moduleName = firstKey.endsWith('Configuration')
                            ? firstKey.slice(0, -13)
                            : firstKey;
                        setExpandedModules(new Set([moduleName]));
                    }
                } else {
                    setError(res.message ?? 'Failed to load configurations');
                }
            })
            .catch((err) => setError(err.message))
            .finally(() => setLoading(false));
    }, []);

    const handleSelectConfig = (name: string) => {
        setSelectedConfig(name);
        setFormValues(configs[name] as Record<string, string | number | boolean>);
        setSaveMessage(null);
    };

    const handleFieldChange = (field: string, value: string | number | boolean) => {
        setFormValues((prev) => ({ ...prev, [field]: value }));
    };

    const handleSave = async () => {
        if (!selectedConfig) return;
        setSaving(true);
        setSaveMessage(null);
        try {
            const payload = {
                configurationType: selectedConfig,
                configuration: formValues,
            };
            const res = await changeSettings(payload as Parameters<typeof changeSettings>[0]);
            if (res.success) {
                setSaveMessage({ type: 'success', text: 'Saved successfully' });
                setConfigs((prev) => ({ ...prev, [selectedConfig]: { ...formValues } }));
            } else {
                setSaveMessage({ type: 'error', text: res.message ?? 'Failed to save' });
            }
        } catch (err) {
            setSaveMessage({
                type: 'error',
                text: err instanceof Error ? err.message : 'Failed to save',
            });
        } finally {
            setSaving(false);
        }
    };

    const groupedConfigs = (): Record<string, string[]> => {
        const groups: Record<string, string[]> = {};
        Object.keys(configs).forEach((name) => {
            const moduleName = name.endsWith('Configuration')
                ? name.slice(0, -13)
                : name;
            if (!groups[moduleName]) groups[moduleName] = [];
            groups[moduleName].push(name);
        });
        return groups;
    };

    if (loading) {
        return (
            <div className="config-editor">
                <div className="config-editor__centered">Loading configurations...</div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="config-editor">
                <div className="config-editor__centered config-editor__error">{error}</div>
            </div>
        );
    }

    const groups = groupedConfigs();

    return (
        <div className="config-editor">
            <aside className="config-editor__sidebar">
                <h2 className="config-editor__sidebar-title">Modules</h2>
                {Object.entries(groups).map(([module, configNames]) => (
                    <div key={module} className="config-editor__group">
                        <button
                            className="config-editor__group-toggle"
                            onClick={() => {
                                setExpandedModules((prev) => {
                                    const next = new Set(prev);
                                    if (next.has(module)) next.delete(module);
                                    else next.add(module);
                                    return next;
                                });
                            }}
                        >
                            <span className="config-editor__group-arrow">
                                {expandedModules.has(module) ? '▼' : '▶'}
                            </span>
                            {module}
                        </button>
                        {expandedModules.has(module) && (
                            <div className="config-editor__group-items">
                                {configNames.map((name) => (
                                    <button
                                        key={name}
                                        className={`config-editor__config-btn ${
                                            selectedConfig === name
                                                ? 'config-editor__config-btn--active'
                                                : ''
                                        }`}
                                        onClick={() => handleSelectConfig(name)}
                                    >
                                        {name}
                                    </button>
                                ))}
                            </div>
                        )}
                    </div>
                ))}
            </aside>
            <main className="config-editor__content">
                {selectedConfig ? (
                    <>
                        <h2 className="config-editor__config-title">{selectedConfig}</h2>
                        <div className="config-editor__fields">
                            {Object.entries(formValues).map(([key, value]) => (
                                <div key={key} className="config-editor__field">
                                    <label className="config-editor__field-label">{key}</label>
                                    {typeof value === 'boolean' ? (
                                        <input
                                            type="checkbox"
                                            className="config-editor__checkbox"
                                            checked={value}
                                            onChange={(e) => handleFieldChange(key, e.target.checked)}
                                        />
                                    ) : typeof value === 'number' ? (
                                        <input
                                            type="number"
                                            className="config-editor__input"
                                            value={value}
                                            onChange={(e) =>
                                                handleFieldChange(key, Number(e.target.value))
                                            }
                                        />
                                    ) : (
                                        <input
                                            type="text"
                                            className="config-editor__input"
                                            value={String(value ?? '')}
                                            onChange={(e) => handleFieldChange(key, e.target.value)}
                                        />
                                    )}
                                </div>
                            ))}
                        </div>
                        <div className="config-editor__actions">
                            <button
                                className="config-editor__save-btn"
                                onClick={handleSave}
                                disabled={saving}
                            >
                                {saving ? 'Saving...' : 'Save'}
                            </button>
                            {saveMessage && (
                                <span
                                    className={`config-editor__message config-editor__message--${saveMessage.type}`}
                                >
                                    {saveMessage.text}
                                </span>
                            )}
                        </div>
                    </>
                ) : (
                    <div className="config-editor__centered">Select a configuration to edit</div>
                )}
            </main>
        </div>
    );
};

register('@admin/configuration', ConfigEditor);
