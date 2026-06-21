import { useState } from "react";
import { getStudySession, updateFlashCardScore, FlashCard } from '@api/flashcard'
import { Spinner } from '@component/Spinner'
import './study.scss'

const PRESETS = [5, 10, 20, 50, 100] as const

interface StudyResult {
    card: FlashCard
    wasCorrect: boolean
}

export const Study = ({ onClose }: { onClose: () => void }) => {

    const [phase, setPhase] = useState<'pick' | 'session' | 'results'>('pick')
    const [cardCount, setCardCount] = useState(10)
    const [customCount, setCustomCount] = useState('')
    const [cards, setCards] = useState<FlashCard[]>([])
    const [currentIndex, setCurrentIndex] = useState(0)
    const [showFront, setShowFront] = useState(true)
    const [userAnswer, setUserAnswer] = useState('')
    const [results, setResults] = useState<StudyResult[]>([])
    const [isLoading, setIsLoading] = useState(false)
    const [error, setError] = useState<string | null>(null)

    const handleStart = async (count: number) => {
        setIsLoading(true)
        setError(null)
        try {
            const response = await getStudySession({ cardCount: count })
            if (!response.success) {
                setError(response.message ?? 'Failed to start study session')
                setIsLoading(false)
                return
            }
            const fetched = response.data ?? []
            if (fetched.length === 0) {
                setError('No cards available for studying')
                setIsLoading(false)
                return
            }
            setCards(fetched)
            setCurrentIndex(0)
            setShowFront(Math.random() < 0.5)
            setUserAnswer('')
            setResults([])
            setPhase('session')
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to start study session')
        } finally {
            setIsLoading(false)
        }
    }

    const handleGrade = async (wasCorrect: boolean) => {
        const card = cards[currentIndex]
        try {
            await updateFlashCardScore({ cardId: card.id, isCorrect: wasCorrect })
        } catch {
            // continue even if persist fails
        }
        setResults((prev) => [...prev, { card, wasCorrect }])
        const nextIndex = currentIndex + 1
        if (nextIndex >= cards.length) {
            setPhase('results')
        } else {
            setCurrentIndex(nextIndex)
            setShowFront(Math.random() < 0.5)
            setUserAnswer('')
        }
    }

    if (phase === 'pick') {
        return (
            <div className={'modal-overlay'} onClick={onClose}>
                <div className={'modal study-pick'} onClick={(e) => e.stopPropagation()}>
                    <div className={'modal-header'}>
                        <h2>How many cards?</h2>
                        <button className={'modal-close'} onClick={onClose}>✕</button>
                    </div>
                    <div className={'modal-body'}>
                        <div className={'preset-grid'}>
                            {PRESETS.map((n) => (
                                <button
                                    key={n}
                                    className={`preset-btn ${cardCount === n && !customCount ? 'active' : ''}`}
                                    onClick={() => { setCardCount(n); setCustomCount('') }}
                                >
                                    {n}
                                </button>
                            ))}
                        </div>
                        <div className={'custom-row'}>
                            <input
                                type={'number'}
                                min={1}
                                max={200}
                                placeholder={'Custom'}
                                value={customCount}
                                onChange={(e) => { setCustomCount(e.target.value); setCardCount(Number(e.target.value) || 10) }}
                            />
                            <button
                                className={'start-btn'}
                                onClick={() => handleStart(cardCount)}
                                disabled={isLoading}
                            >
                                {isLoading ? 'Starting...' : 'Start'}
                            </button>
                        </div>
                        {error && <div className={'modal-error'}>{error}</div>}
                    </div>
                </div>
            </div>
        )
    }

    if (phase === 'results') {
        const correctCount = results.filter((r) => r.wasCorrect).length
        return (
            <div className={'modal-overlay'}>
                <div className={'modal study-results'} onClick={(e) => e.stopPropagation()}>
                    <div className={'modal-header'}>
                        <h2>Study Results</h2>
                        <button className={'modal-close'} onClick={onClose}>✕</button>
                    </div>
                    <div className={'modal-body'}>
                        <div className={'results-summary'}>
                            {correctCount} of {results.length} correct ({Math.round((correctCount / results.length) * 100)}%)
                        </div>
                        <div className={'results-list'}>
                            {results.map((r, i) => (
                                <div key={i} className={`result-item ${r.wasCorrect ? 'correct' : 'incorrect'}`}>
                                    <span className={'result-icon'}>{r.wasCorrect ? '✓' : '✗'}</span>
                                    <span className={'result-front'}>{r.card.frontStatement}</span>
                                    <span className={'result-arrow'}>→</span>
                                    <span className={'result-back'}>{r.card.backStatement}</span>
                                </div>
                            ))}
                        </div>
                    </div>
                    <div className={'modal-actions'}>
                        <button onClick={onClose}>Close</button>
                    </div>
                </div>
            </div>
        )
    }

    if (isLoading) {
        return <Spinner />
    }

    if (error) {
        return (
            <div className={'modal-overlay'}>
                <div className={'modal'}>
                    <div className={'modal-body'}>
                        <div className={'modal-error'}>{error}</div>
                    </div>
                    <div className={'modal-actions'}>
                        <button onClick={onClose}>Close</button>
                    </div>
                </div>
            </div>
        )
    }

    const card = cards[currentIndex]
    const shown = showFront ? card.frontStatement : card.backStatement

    return (
        <div className={'study-session'}>
            <div className={'session-header'}>
                <span className={'progress'}>{currentIndex + 1} / {cards.length}</span>
                <button className={'end-btn'} onClick={() => setPhase('results')}>End Session</button>
            </div>
            <div className={'session-card'}>
                <div className={'card-label'}>{showFront ? 'Front' : 'Back'}</div>
                <div className={'card-text'}>{shown}</div>
            </div>
            <div className={'session-input-area'}>
                <label>Type the {showFront ? 'back' : 'front'}:</label>
                <input
                    type={'text'}
                    value={userAnswer}
                    onChange={(e) => setUserAnswer(e.target.value)}
                    placeholder={`Enter ${showFront ? 'back' : 'front'} statement...`}
                />
            </div>
            <div className={'grade-buttons'}>
                <button className={'incorrect-btn'} onClick={() => handleGrade(false)}>Incorrect</button>
                <button className={'correct-btn'} onClick={() => handleGrade(true)}>Correct</button>
            </div>
        </div>
    )
}
