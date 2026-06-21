import { useState, MouseEvent } from "react";
import { useLanguage } from '@hook/language-provider'
import { useSession } from '@hook/session-provider'
import { usePagination } from '@hook/usePagination'
import {
    listFlashCardPagedSorted,
    createFlashCard,
    deleteFlashCard,
    FlashCard
} from '@api/flashcard'
import { Spinner } from '@component/Spinner'
import { CreateFlashCardModal } from './CreateFlashCardModal'
import { Study } from './Study'
import './style.scss'

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
    } = usePagination<FlashCard[]>(listFlashCardPagedSorted, [language, isModalOpen, isDeleting])

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
        return <Spinner />
    }

    if (flashCardsError) {
        return (
            <div className={'error'}>{`An error occured loading your flash cards: ${flashCardsError}`}</div>
        )
    }

    const cards = flashCards ?? []
    const userId = session?.user?.id
    const languageId = language?.id

    return (
        <div className={'flashcards'}>
            <div className={'card-actions'}>
                {page <= 1 ? null : <button onClick={prevPage} disabled={page <= 1}>Previous</button>}
                <span>{`Page ${page}`}</span>
                {cards.length > size ? <button onClick={nextPage}>Next</button> : null}
                <select value={sortField ?? ''} onChange={(e) => setSortField(e.target.value || undefined)}>
                    <option value="">Sort by...</option>
                    <option value="frontStatement">Front</option>
                    <option value="backStatement">Back</option>
                    <option value="difficulty">Difficulty</option>
                    <option value="reviewCount">Reviews</option>
                    <option value="createTime">Created</option>
                </select>
                <select value={sortDir ?? 'asc'} onChange={(e) => setSortDir(e.target.value)}>
                    <option value="asc">Asc</option>
                    <option value="desc">Desc</option>
                </select>
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
                    disabled={!userId || !languageId}
                >
                    Study
                </button>
            </div>

            {Boolean(!cards || cards.length === 0) && (
                <div className={'notice'}>You have no flashcards, click create new to begin</div>
            )}

            {cards && cards.length > 0 && (
                <div className={'card-grid'}>
                    {cards.map((card) => {
                        const isFlipped = flippedIds.has(card.id)
                        return (
                            <div
                                key={card.id}
                                className={`flashcard ${isFlipped ? 'flipped' : ''}`}
                                onClick={() => toggleFlip(card.id)}
                            >
                                <div className={'flashcard-inner'}>
                                    <div className={'flashcard-face flashcard-front'}>
                                        <div className={'statement'}>{card.frontStatement}</div>
                                        {card.category && (
                                            <div className={'tag category'}>{card.category}</div>
                                        )}
                                        <button
                                            className={'delete-button'}
                                            onClick={(e) => handleDelete(card, e)}
                                            disabled={isDeleting === card.id}
                                        >
                                            {isDeleting === card.id ? '...' : '✕'}
                                        </button>
                                    </div>
                                    <div className={'flashcard-face flashcard-back'}>
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
                                        <div className={'difficulty'}>
                                            {'Difficulty: '}{card.difficulty}
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