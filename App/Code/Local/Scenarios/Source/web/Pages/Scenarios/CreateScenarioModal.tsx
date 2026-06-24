import { useState, FormEvent } from "react";
import { createScenario, Scenario } from '@api/scenario';
import './modal-style.scss';

interface CreateScenarioModalProps {
    onClose: () => void;
    onCreated: () => void;
}

interface FormState {
    title: string;
    steps: string;
}

const emptyForm: FormState = {
    title: '',
    steps: '',
};

export const CreateScenarioModal = ({ onClose, onCreated }: CreateScenarioModalProps) => {
    const [form, setForm] = useState<FormState>(emptyForm);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const updateField = <K extends keyof FormState>(field: K, value: FormState[K]) => {
        setForm((prev) => ({ ...prev, [field]: value }));
    };

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        if (!form.title.trim() || !form.steps.trim()) {
            setError('Title and steps are required');
            return;
        }

        setIsSubmitting(true);
        setError(null);

        try {
            const payload: Partial<Scenario> = {
                title: form.title,
                steps: form.steps
            };

            const response = await createScenario(payload as Scenario);

            if (!response.success) {
                setError(response.message ?? 'Failed to create scenario');
                return;
            }

            onCreated();
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to create scenario');
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className={'modal-overlay'} onClick={onClose}>
            <div className={'modal'} onClick={(e) => e.stopPropagation()}>
                <div className={'modal-header'}>
                    <h2>Create new scenario</h2>
                    <button className={'modal-close'} onClick={onClose}>✕</button>
                </div>

                <form onSubmit={handleSubmit} className={'modal-body'}>
                    <label>
                        Title
                        <input
                            type={'text'}
                            value={form.title}
                            onChange={(e) => updateField('title', e.target.value)}
                            required
                        />
                    </label>

                    <label>
                        Steps
                        <textarea
                            value={form.steps}
                            onChange={(e) => updateField('steps', e.target.value)}
                            required
                            placeholder={'Enter each step on a new line...'}
                        />
                    </label>

                    {error && <div className={'modal-error'}>{error}</div>}

                    <div className={'modal-actions'}>
                        <button type={'button'} onClick={onClose} disabled={isSubmitting}>
                            Cancel
                        </button>
                        <button type={'submit'} disabled={isSubmitting}>
                            {isSubmitting ? 'Creating...' : 'Create'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
};