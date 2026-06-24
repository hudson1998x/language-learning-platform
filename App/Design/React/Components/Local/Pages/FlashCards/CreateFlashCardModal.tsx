import { useState, FormEvent } from "react";
import { createFlashCard, FlashCard } from '@api/flashcard'
import { LanguageSelector } from '@component/LanguageSelector'
import { generatePronunciation } from '@api/homeChat'
import './modal-style.scss'

interface CreateFlashCardModalProps {
    userId: string;
    languageId: string;
    onClose: () => void;
    onCreated: () => void;
    initialValues?: Partial<FormState>;
    showLanguageSelector?: boolean;
}

interface FormState {
    frontStatement: string;
    backStatement: string;
    pronunciation: string;
    notes: string;
    category: string;
    tags: string;
    difficulty: number;
}

const emptyForm: FormState = {
    frontStatement: '',
    backStatement: '',
    pronunciation: '',
    notes: '',
    category: '',
    tags: '',
    difficulty: 1,
}

const DIFFICULTY_OPTIONS = [
    { value: 1, label: 'Easy' },
    { value: 2, label: 'Mid' },
    { value: 3, label: 'Hard' },
]

export const CreateFlashCardModal = ({ userId, languageId, onClose, onCreated, initialValues, showLanguageSelector }: CreateFlashCardModalProps) => {

    const [form, setForm] = useState<FormState>(() => ({ ...emptyForm, ...initialValues }))
    const [selectedLanguageId, setSelectedLanguageId] = useState<string | null>(languageId || null)
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [isGeneratingPronunciation, setIsGeneratingPronunciation] = useState(false)
    const [error, setError] = useState<string | null>(null)

    const updateField = <K extends keyof FormState>(field: K, value: FormState[K]) => {
        setForm((prev) => ({ ...prev, [field]: value }))
    }

    const handleGeneratePronunciation = async () => {
        if (!form.frontStatement.trim() || isGeneratingPronunciation) return;

        setIsGeneratingPronunciation(true);
        try {
            const res = await generatePronunciation({ text: form.frontStatement });
            if (res.success && res.data?.pronunciation) {
                updateField('pronunciation', res.data.pronunciation);
            }
        } catch {
            // silent — field stays empty, user can type manually
        } finally {
            setIsGeneratingPronunciation(false);
        }
    };

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault()

        if (!form.frontStatement.trim() || !form.backStatement.trim()) {
            setError('Front and back statements are required')
            return
        }

        const langId = selectedLanguageId ?? languageId
        if (!langId) {
            setError('Please select a language')
            return
        }

        setIsSubmitting(true)
        setError(null)

        try {
            const payload: FlashCard = {
                userId,
                languageId: langId,
                frontStatement: form.frontStatement,
                backStatement: form.backStatement,
                pronunciation: form.pronunciation || null,
                notes: form.notes || null,
                category: form.category || null,
                tags: form.tags || null,
                difficulty: form.difficulty,
                reviewCount: 0,
                correctCount: 0,
                incorrectCount: 0,
                streak: 0,
            }

            const response = await createFlashCard(payload)

            if (!response.success) {
                setError(response.message ?? 'Failed to create flash card')
                return
            }

            onCreated()
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to create flash card')
        } finally {
            setIsSubmitting(false)
        }
    }

    return (
        <div className={'modal-overlay'} onClick={onClose}>
            <div className={'modal'} onClick={(e) => e.stopPropagation()}>
                <div className={'modal-header'}>
                    <h2>Create flash card</h2>
                    <button className={'modal-close'} onClick={onClose}>✕</button>
                </div>

                <form onSubmit={handleSubmit} className={'modal-body'}>
                    {showLanguageSelector && (
                        <label className={'language-field'}>
                            Language
                            <LanguageSelector
                                onLanguageChange={(lang) => setSelectedLanguageId(lang.id)}
                            />
                        </label>
                    )}
                    <label>
                        Front statement
                        <textarea
                            value={form.frontStatement}
                            onChange={(e) => updateField('frontStatement', e.target.value)}
                            required
                        />
                    </label>

                    <label>
                        Back statement
                        <textarea
                            value={form.backStatement}
                            onChange={(e) => updateField('backStatement', e.target.value)}
                            required
                        />
                    </label>

                    <label>
                        Pronunciation
                        <div className={'pronunciation-row'}>
                            <input
                                type={'text'}
                                value={form.pronunciation}
                                onChange={(e) => updateField('pronunciation', e.target.value)}
                            />
                            {!form.pronunciation.trim() && form.frontStatement.trim() && (
                                <button
                                    type={'button'}
                                    className={'pronounce-btn'}
                                    onClick={handleGeneratePronunciation}
                                    disabled={isGeneratingPronunciation}
                                >
                                    {isGeneratingPronunciation ? '...' : 'Generate'}
                                </button>
                            )}
                        </div>
                    </label>

                    <label>
                        Notes
                        <textarea
                            value={form.notes}
                            onChange={(e) => updateField('notes', e.target.value)}
                        />
                    </label>

                    <label>
                        Category
                        <input
                            type={'text'}
                            value={form.category}
                            onChange={(e) => updateField('category', e.target.value)}
                        />
                    </label>

                    <label>
                        Tags
                        <input
                            type={'text'}
                            placeholder={'comma, separated, tags'}
                            value={form.tags}
                            onChange={(e) => updateField('tags', e.target.value)}
                        />
                    </label>

                    <label>
                        Difficulty
                        <select
                            value={form.difficulty}
                            onChange={(e) => updateField('difficulty', Number(e.target.value))}
                        >
                            {DIFFICULTY_OPTIONS.map((option) => (
                                <option key={option.value} value={option.value}>
                                    {option.label}
                                </option>
                            ))}
                        </select>
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
    )
}