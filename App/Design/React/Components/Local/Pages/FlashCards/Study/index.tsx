import { useEffect, useMemo, useRef, useState } from "react";
import { getStudySession, updateFlashCardScore, FlashCard } from '@api/flashcard'
import { Spinner } from '@component/Spinner'
import './study.scss'

const PRESETS = [5, 10, 20, 50, 100] as const
const MAX_CUSTOM = 200

interface StudyResult {
    card: FlashCard
    wasCorrect: boolean
    showedFront: boolean
}

// Loose match: ignores case, surrounding whitespace, and repeated
// internal spaces, so "Paris" vs " paris  " both count.
const normalize = (s: string) => s.trim().toLowerCase().replace(/\s+/g, ' ')

export const Study = ({ onClose }: { onClose: () => void }) => {

    const [phase, setPhase] = useState<'pick' | 'session' | 'results'>('pick')
    const [cardCount, setCardCount] = useState(10)
    const [customCount, setCustomCount] = useState('')
    const [cards, setCards] = useState<FlashCard[]>([])
    const [currentIndex, setCurrentIndex] = useState(0)
    const [showFront, setShowFront] = useState(true)
    const [userAnswer, setUserAnswer] = useState('')
    const [checked, setChecked] = useState(false)
    const [results, setResults] = useState<StudyResult[]>([])
    const [isLoading, setIsLoading] = useState(false)
    const [error, setError] = useState<string | null>(null)
    const inputRef = useRef<HTMLInputElement>(null)

    const card = cards[currentIndex]
    const expectedAnswer = card ? (showFront ? card.backStatement : card.frontStatement) : ''
    const isAnswerCorrect = checked && normalize(userAnswer) === normalize(expectedAnswer)

    const startSession = async (count: number) => {
        const clamped = Math.min(Math.max(count, 1), MAX_CUSTOM)
        setIsLoading(true)
        setError(null)
        try {
            const response = await getStudySession({ cardCount: clamped })
            if (!response.success) {
                setError(response.message ?? 'Failed to start study session')
                return
            }
            const fetched = response.data ?? []
            if (fetched.length === 0) {
                setError('No cards available for studying')
                return
            }
            setCards(fetched)
            setCurrentIndex(0)
            setShowFront(Math.random() < 0.5)
            setUserAnswer('')
            setChecked(false)
            setResults([])
            setPhase('session')
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to start study session')
        } finally {
            setIsLoading(false)
        }
    }

    const goToCard = (deck: FlashCard[], index: number) => {
        setCards(deck)
        setCurrentIndex(index)
        setShowFront(Math.random() < 0.5)
        setUserAnswer('')
        setChecked(false)
    }

    const handleCheck = () => {
        if (!userAnswer.trim()) return
        setChecked(true)
    }

    const handleGrade = async (wasCorrect: boolean) => {
        try {
            await updateFlashCardScore({ cardId: card.id, isCorrect: wasCorrect })
        } catch {
            // continue even if persist fails
        }
        setResults((prev) => [...prev, { card, wasCorrect, showedFront: showFront }])
        const nextIndex = currentIndex + 1
        if (nextIndex >= cards.length) {
            setPhase('results')
        } else {
            goToCard(cards, nextIndex)
        }
    }

    const handleRetryMissed = () => {
        const missed = results.filter((r) => !r.wasCorrect).map((r) => r.card)
        if (missed.length === 0) return
        setResults([])
        goToCard(missed, 0)
        setPhase('session')
    }

    // Keyboard shortcuts during an active session: Enter checks the
    // answer (or advances once checked), 1/← marks incorrect, 2/→
    // marks correct once the answer's been checked.
    useEffect(() => {
        if (phase !== 'session' || isLoading || error) return
        const onKeyDown = (e: KeyboardEvent) => {
            if (e.key === 'Enter') {
                e.preventDefault()
                if (!checked) handleCheck()
                return
            }
            if (!checked) return
            if (e.key === '1' || e.key === 'ArrowLeft') {
                e.preventDefault()
                handleGrade(false)
            } else if (e.key === '2' || e.key === 'ArrowRight') {
                e.preventDefault()
                handleGrade(true)
            }
        }
        window.addEventListener('keydown', onKeyDown)
        return () => window.removeEventListener('keydown', onKeyDown)
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, [phase, isLoading, error, checked, userAnswer, currentIndex])

    useEffect(() => {
        if (phase === 'session' && !checked) inputRef.current?.focus()
    }, [phase, checked, currentIndex])

    const missedCount = useMemo(() => results.filter((r) => !r.wasCorrect).length, [results])

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
                                max={MAX_CUSTOM}
                                placeholder={'Custom'}
                                value={customCount}
                                onChange={(e) => { setCustomCount(e.target.value); setCardCount(Number(e.target.value) || 10) }}
                            />
                            <button
                                className={'start-btn'}
                                onClick={() => startSession(cardCount)}
                                disabled={isLoading}
                            >
                                {isLoading ? 'Starting...' : 'Start'}
                            </button>
                        </div>
                        <div className={'hint-row'}>Up to {MAX_CUSTOM} cards per session</div>
                        {error && <div className={'modal-error'}>{error}</div>}
                    </div>
                </div>
            </div>
        )
    }

    if (phase === 'results') {
        const correctCount = results.filter((r) => r.wasCorrect).length
        const pct = results.length ? Math.round((correctCount / results.length) * 100) : 0
        return (
            <div className={'study-results study-fullscreen'}>
                <div className={'results-content'}>
                    <div className={'results-header'}>
                        <h2>Session complete</h2>
                    </div>
                    <div className={'results-summary'}>
                        <div className={`summary-pct ${pct >= 70 ? 'summary-good' : 'summary-bad'}`}>{pct}%</div>
                        <div className={'summary-detail'}>{correctCount} of {results.length} correct</div>
                    </div>
                    <div className={'results-list'}>
                        {results.map((r, i) => (
                            <div key={i} className={`result-item ${r.wasCorrect ? 'correct' : 'incorrect'}`}>
                                <span className={'result-icon'}>{r.wasCorrect ? '✓' : '✗'}</span>
                                <span className={'result-front'}>{r.showedFront ? r.card.frontStatement : r.card.backStatement}</span>
                                <span className={'result-arrow'}>→</span>
                                <span className={'result-back'}>{r.showedFront ? r.card.backStatement : r.card.frontStatement}</span>
                            </div>
                        ))}
                    </div>
                    <div className={'results-actions'}>
                        {missedCount > 0 && (
                            <button onClick={handleRetryMissed}>Retry {missedCount} missed</button>
                        )}
                        <button className={'primary'} onClick={onClose}>Done</button>
                    </div>
                </div>
            </div>
        )
    }

    if (isLoading) {
        return (
            <div className={'study-fullscreen'}>
                <div className={'session-status'}><Spinner /></div>
            </div>
        )
    }

    if (error) {
        return (
            <div className={'study-fullscreen'}>
                <div className={'session-status'}>
                    <div>
                        <div className={'modal-error'}>{error}</div>
                        <div className={'results-actions'} style={{ marginTop: 16 }}>
                            <button className={'primary'} onClick={onClose}>Close</button>
                        </div>
                    </div>
                </div>
            </div>
        )
    }

    if (!card) return null

    const shown = showFront ? card.frontStatement : card.backStatement

    return (
        <div className={'study-fullscreen'}>
            <div className={'session-track'}>
                <div className={'track-top'}>
                    <span className={'track-meta'}><strong>{currentIndex + 1}</strong> / {cards.length}</span>
                    <button className={'exit-btn'} onClick={() => setPhase('results')}>End session</button>
                </div>
                <div className={'track-bar'}>
                    {cards.map((_, i) => {
                        const result = results[i]
                        let segClass = ''
                        if (result) segClass = result.wasCorrect ? 'done-correct' : 'done-incorrect'
                        else if (i === currentIndex) segClass = 'current'
                        return <div key={i} className={`track-seg ${segClass}`} />
                    })}
                </div>
            </div>

            <div className={'session-stage'}>
                <div className={'session-card'}>
                    <div className={'card-label'}>{showFront ? 'Front' : 'Back'}</div>
                    <div className={'card-text'}>{shown}</div>
                </div>

                <div className={'session-input-area'}>
                    <label>Type the {showFront ? 'back' : 'front'}:</label>
                    <input
                        ref={inputRef}
                        type={'text'}
                        value={userAnswer}
                        disabled={checked}
                        onChange={(e) => setUserAnswer(e.target.value)}
                        placeholder={`Enter ${showFront ? 'back' : 'front'} statement...`}
                        className={checked ? (isAnswerCorrect ? 'input-correct' : 'input-incorrect') : ''}
                    />
                    {checked && (
                        <div className={`answer-feedback ${isAnswerCorrect ? 'is-correct' : 'is-incorrect'}`}>
                            <span className={'feedback-label'}>{isAnswerCorrect ? 'Matched' : 'Answer:'}</span>
                            {!isAnswerCorrect && <span className={'feedback-value'}>{expectedAnswer}</span>}
                        </div>
                    )}
                </div>
            </div>

            <div className={'session-actions'}>
                <div className={'actions-inner'}>
                    {!checked ? (
                        <>
                            <button className={'check-btn'} onClick={handleCheck} disabled={!userAnswer.trim()}>
                                Check answer
                            </button>
                            <span className={'kbd-hint'}><kbd>Enter</kbd></span>
                        </>
                    ) : (
                        <>
                            <button className={'incorrect-btn'} onClick={() => handleGrade(false)}>Incorrect</button>
                            <button className={'correct-btn'} onClick={() => handleGrade(true)}>Correct</button>
                            <span className={'kbd-hint'}><kbd>1</kbd><kbd>2</kbd></span>
                        </>
                    )}
                </div>
            </div>
        </div>
    )
}