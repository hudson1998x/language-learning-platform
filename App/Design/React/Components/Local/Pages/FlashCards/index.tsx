import { useState, MouseEvent } from "react";
import { useLanguage } from '@hook/language-provider'
import { useSession } from '@hook/session-provider'
import { usePagination } from '@hook/usePagination'
import {
    listFlashCardsForLanguage,
    deleteFlashCard,
    FlashCard
} from '@api/flashcard'
import { Spinner } from '@component/Spinner'
import { CreateFlashCardModal } from './CreateFlashCardModal'
import { Study } from './Study'
import './style.scss'

const MAX_DIFFICULTY = 5

const SORT_OPTIONS = [
    { value: 'createTime', label: 'Date created' },
    { value: 'frontStatement', label: 'Front text' },
    { value: 'backStatement', label: 'Back text' },
    { value: 'difficulty', label: 'Difficulty' },
    { value: 'reviewCount', label: 'Reviews' },
] as const

const formatLastReviewed = (iso?: string | null): string => {
    if (!iso) return 'Not studied yet'
    const date = new Date(iso)
    const days = Math.floor((Date.now() - date.getTime()) / 86_400_000)
    if (days <= 0) return 'Studied today'
    if (days === 1) return 'Studied yesterday'
    if (days < 30) return `Studied ${days}d ago`
    const months = Math.floor(days / 30)
    return `Studied ${months}mo ago`
}

const accuracyOf = (card: FlashCard): number | null => {
    const total = card.correctCount + card.incorrectCount
    if (total === 0) return null
    return Math.round((card.correctCount / total) * 100)
}

export const FlashCards = () => {

    const { language } = useLanguage()
    const { session } = useSession()

    const [isModalOpen, setIsModalOpen] = useState(false)
    const [isDeleting, setIsDeleting] = useState<string | null>(null)
    const [isStudyActive, setIsStudyActive] = useState(false)

    const {
        page,
        size,
        nextPage,
        prevPage,
        results: flashCards,
        isLoading: flashCardsLoading,
        error: flashCardsError,
        sortField,
        sortDir,
        setSortField,
        setSortDir,
    } = usePagination<FlashCard[]>(listFlashCardsForLanguage, [language, isModalOpen, isDeleting])

    const [flippedIds, setFlippedIds] = useState<Set<string>>(new Set())

    const toggleFlip = (id: string) => {
        setFlippedIds((prev) => {
            const next = new Set(prev)
            if (next.has(id)) {
                next.delete(id)
            } else {
                next.add(id)
            }
            return next
        })
    }

    const handleDelete = async (card: FlashCard, e: MouseEvent) => {
        e.stopPropagation()
        setIsDeleting(card.id)
        try {
            await deleteFlashCard(card)
        } finally {
            setIsDeleting(null)
        }
    }

    if (flashCardsLoading) {
        return (
            <div className={'flashcards-status'}><Spinner /></div>
        )
    }

    if (flashCardsError) {
        return (
            <div className={'flashcards-status'}>
                <div className={'error'}>{`Couldn't load your flashcards: ${flashCardsError}`}</div>
            </div>
        )
    }

    const cards = flashCards ?? []
    const userId = session?.user?.id
    const languageId = language?.id
    // The pagination hook has no total count — a full page is the only
    // signal we have that another page might exist.
    const mayHaveNextPage = cards.length === size

    return (
        <div className={'flashcards'}>
            <div className={'card-actions'}>
                <div className={'pagination'}>
                    <button onClick={prevPage} disabled={page <= 1}>Previous</button>
                    <span>{`Page ${page}`}</span>
                    <button onClick={nextPage} disabled={!mayHaveNextPage}>Next</button>
                </div>

                <div className={'sort-controls'}>
                    <select value={sortField ?? ''} onChange={(e) => setSortField(e.target.value || undefined)}>
                        {SORT_OPTIONS.map((opt) => (
                            <option key={opt.value} value={opt.value}>{opt.label}</option>
                        ))}
                    </select>
                    <select value={sortDir ?? 'desc'} onChange={(e) => setSortDir(e.target.value)}>
                        <option value="asc">Asc</option>
                        <option value="desc">Desc</option>
                    </select>
                </div>

                <div className={'primary-actions'}>
                    <button
                        className={'create-button'}
                        onClick={() => setIsModalOpen(true)}
                        disabled={!userId || !languageId}
                    >
                        Create new
                    </button>
                    <button
                        className={'study-button'}
                        onClick={() => setIsStudyActive(true)}
                        disabled={!userId || !languageId || cards.length === 0}
                    >
                        Study
                    </button>
                </div>
            </div>

            {cards.length === 0 && (
                <div className={'notice'}>
                    <div className={'notice-title'}>No flashcards yet</div>
                    <div className={'notice-body'}>Click "Create new" to add your first card.</div>
                </div>
            )}

            {cards.length > 0 && (
                <div className={'card-grid'}>
                    {cards.map((card) => {
                        const isFlipped = flippedIds.has(card.id)
                        const masteryTicks = Math.max(0, Math.min(MAX_DIFFICULTY, MAX_DIFFICULTY - card.difficulty))
                        const accuracy = accuracyOf(card)
                        return (
                            <div
                                key={card.id}
                                className={`flashcard ${isFlipped ? 'flipped' : ''}`}
                                onClick={() => toggleFlip(card.id)}
                                role={'button'}
                                tabIndex={0}
                                onKeyDown={(e) => {
                                    if (e.key === 'Enter' || e.key === ' ') {
                                        e.preventDefault()
                                        toggleFlip(card.id)
                                    }
                                }}
                            >
                                <div className={'flashcard-inner'}>
                                    <div className={'flashcard-face flashcard-front'}>
                                        <button
                                            className={'delete-button'}
                                            onClick={(e) => handleDelete(card, e)}
                                            disabled={isDeleting === card.id}
                                            aria-label={'Delete card'}
                                        >
                                            {isDeleting === card.id ? '...' : '✕'}
                                        </button>
                                        {card.category && (
                                            <div className={'tag category'}>{card.category}</div>
                                        )}

                                        <div className={'face-body'}>
                                            <div className={'statement'}>{card.frontStatement}</div>
                                        </div>

                                        <div className={'face-footer'}>
                                            <div className={'mastery'} title={`Difficulty ${card.difficulty}/${MAX_DIFFICULTY}`}>
                                                {Array.from({ length: MAX_DIFFICULTY }).map((_, i) => (
                                                    <span key={i} className={`mastery-tick ${i < masteryTicks ? 'filled' : ''}`} />
                                                ))}
                                            </div>

                                            {accuracy !== null && (
                                                <span className={`stat stat-accuracy ${accuracy >= 70 ? 'good' : 'weak'}`}>
                                                    {accuracy}% accurate
                                                </span>
                                            )}
                                            <div className={'stat-row'}>
                                                <span className={'stat'}>
                                                    {card.reviewCount} {card.reviewCount === 1 ? 'review' : 'reviews'}
                                                </span>
                                                {card.streak > 0 && (
                                                    <span className={'stat stat-streak'}>🔥 {card.streak}</span>
                                                )}
                                            </div>
                                        </div>
                                    </div>

                                    <div className={'flashcard-face flashcard-back'}>
                                        <div className={'face-body'}>
                                            <div className={'statement'}>{card.backStatement}</div>
                                            {card.pronunciation && (
                                                <div className={'pronunciation'}>{card.pronunciation}</div>
                                            )}
                                            {card.notes && (
                                                <div className={'notes'}>{card.notes}</div>
                                            )}
                                            {card.tags && (
                                                <div className={'tag tags'}>{card.tags}</div>
                                            )}
                                        </div>

                                        <div className={'face-footer'}>
                                            <span className={'stat'}>{formatLastReviewed(card.lastReviewedUtc)}</span>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        )
                    })}
                </div>
            )}

            {isModalOpen && userId && languageId && (
                <CreateFlashCardModal
                    userId={userId}
                    languageId={languageId}
                    onClose={() => setIsModalOpen(false)}
                    onCreated={() => setIsModalOpen(false)}
                />
            )}

            {isStudyActive && (
                <Study onClose={() => setIsStudyActive(false)} />
            )}
        </div>
    )
}