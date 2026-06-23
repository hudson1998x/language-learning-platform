import { useState, FormEvent } from "react";
import { translateSong } from '@api/musicTranslation'

interface CreateSongModalProps {
    onClose: () => void;
    onCreated: (trackId: string) => void;
}

interface FormState {
    lyrics: string;
    title: string;
    artist: string;
    album: string;
}

const emptyForm: FormState = {
    lyrics: '',
    title: '',
    artist: '',
    album: '',
}

export const CreateSongModal = ({ onClose, onCreated }: CreateSongModalProps) => {
    const [form, setForm] = useState<FormState>(emptyForm)
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [error, setError] = useState<string | null>(null)

    const updateField = <K extends keyof FormState>(field: K, value: FormState[K]) => {
        setForm((prev) => ({ ...prev, [field]: value }))
    }

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault()

        if (!form.lyrics.trim() || !form.title.trim() || !form.artist.trim() || !form.album.trim()) {
            setError('All fields are required')
            return
        }

        setIsSubmitting(true)
        setError(null)

        try {
            const response = await translateSong({
                lyrics: form.lyrics,
                title: form.title,
                artist: form.artist,
                album: form.album,
            })

            if (!response.success || !response.data) {
                setError(response.message ?? 'Failed to translate song')
                return
            }

            onCreated(response.data.song.id)
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to translate song')
        } finally {
            setIsSubmitting(false)
        }
    }

    return (
        <div className={'modal-overlay'} onClick={onClose}>
            <div className={'modal'} onClick={(e) => e.stopPropagation()}>
                <div className={'modal-header'}>
                    <h2>Translate a song</h2>
                    <button className={'modal-close'} onClick={onClose}>✕</button>
                </div>

                <form onSubmit={handleSubmit} className={'modal-body'}>
                    <label>
                        Song lyrics
                        <textarea
                            value={form.lyrics}
                            onChange={(e) => updateField('lyrics', e.target.value)}
                            placeholder={'Paste the song lyrics here...'}
                            required
                        />
                    </label>

                    <label>
                        Title
                        <input
                            type={'text'}
                            value={form.title}
                            onChange={(e) => updateField('title', e.target.value)}
                            placeholder={'Song title'}
                            required
                        />
                    </label>

                    <label>
                        Artist
                        <input
                            type={'text'}
                            value={form.artist}
                            onChange={(e) => updateField('artist', e.target.value)}
                            placeholder={'Artist name'}
                            required
                        />
                    </label>

                    <label>
                        Album
                        <input
                            type={'text'}
                            value={form.album}
                            onChange={(e) => updateField('album', e.target.value)}
                            placeholder={'Album name'}
                            required
                        />
                    </label>

                    {error && <div className={'modal-error'}>{error}</div>}

                    <div className={'modal-actions'}>
                        <button type={'button'} onClick={onClose} disabled={isSubmitting}>
                            Cancel
                        </button>
                        <button type={'submit'} disabled={isSubmitting}>
                            {isSubmitting ? 'Translating...' : 'Translate'}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    )
}
