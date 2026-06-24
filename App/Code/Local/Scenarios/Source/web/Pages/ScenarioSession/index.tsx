import { useEffect, useRef, useState } from "react";
import {
    startStudySession,
    sendMessage,
    StartStudySessionResponse,
    ScenarioLine,
    ChatMessage
} from '@api/scenario';
import { Spinner } from '@component/Spinner';
import { useSession } from '@hook/session-provider';
import { useLanguage } from '@hook/language-provider';
import { CreateFlashCardModal } from '@component/Pages/FlashCards/CreateFlashCardModal';
import './style.scss';

const DIFFICULTIES = [
    { value: 1, label: 'Easy', desc: 'Simple vocabulary, short sentences' },
    { value: 2, label: 'Medium', desc: 'Moderate vocabulary, natural pacing' },
    { value: 3, label: 'Hard', desc: 'Complex vocabulary, idiomatic expressions' },
] as const;

interface ChatEntry {
    line: ScenarioLine;
}

interface ScenarioSessionProps {
    scenarioId: string;
    onClose: () => void;
}

export const ScenarioSession = ({ scenarioId, onClose }: ScenarioSessionProps) => {
    const { session } = useSession();
    const { language } = useLanguage();

    const [phase, setPhase] = useState<'pick-difficulty' | 'chat' | 'error'>('pick-difficulty');
    const [scenario, setScenario] = useState<StartStudySessionResponse | null>(null);
    const [chatEntries, setChatEntries] = useState<ChatEntry[]>([]);
    const [userInput, setUserInput] = useState('');
    const [isSending, setIsSending] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [openMenuIndex, setOpenMenuIndex] = useState<number | null>(null);
    const [flashcardIndex, setFlashcardIndex] = useState<number | null>(null);
    const [correctionIndex, setCorrectionIndex] = useState<number | null>(null);

    const messagesEndRef = useRef<HTMLDivElement>(null);
    const inputRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [chatEntries]);

    useEffect(() => {
        if (phase === 'chat' && !isSending) {
            inputRef.current?.focus();
        }
    }, [phase, isSending]);

    useEffect(() => {
        const handleEsc = (e: KeyboardEvent) => {
            if (e.key === 'Escape') {
                if (openMenuIndex !== null) {
                    setOpenMenuIndex(null);
                } else {
                    onClose();
                }
            }
        };
        window.addEventListener('keydown', handleEsc);
        return () => window.removeEventListener('keydown', handleEsc);
    }, [onClose, openMenuIndex]);

    useEffect(() => {
        if (openMenuIndex === null) return;
        const handleClick = () => setOpenMenuIndex(null);
        window.addEventListener('click', handleClick);
        return () => window.removeEventListener('click', handleClick);
    }, [openMenuIndex]);

    const handleStartSession = async (difficulty: number) => {
        setIsSending(true);
        setError(null);
        try {
            const res = await startStudySession({ scenarioId, difficulty });
            if (!res.success || !res.data) {
                setError(res.message ?? 'Failed to start session');
                return;
            }
            setScenario(res.data);
            setChatEntries([]);
            setPhase('chat');
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to start session');
        } finally {
            setIsSending(false);
        }
    };

    const handleSend = async () => {
        if (!userInput.trim() || !scenario || isSending) return;

        const userLine: ScenarioLine = {
            original: userInput,
            message: userInput,
            translation: '',
            pronunciation: '',
            culturalMeaning: '',
            hint: '',
            isUser: true,
        };

        setChatEntries((prev) => [...prev, { line: userLine }]);
        const messageText = userInput;
        setUserInput('');
        setIsSending(true);
        setError(null);

        try {
            const history = chatEntries.map((entry) => {
                const base: Record<string, unknown> = {
                    role: entry.line.isUser ? 'user' : 'assistant',
                    content: entry.line.isUser ? entry.line.original : entry.line.message,
                };
                if (!entry.line.isUser) {
                    const ai = entry.line as Record<string, unknown>;
                    base.correct = ai.correct;
                    base.feedback = ai.feedback;
                    base.hint = ai.hint;
                }
                return base as unknown as ChatMessage;
            });

            const res = await sendMessage({
                message: messageText,
                scenarioTitle: scenario.title,
                scenarioSteps: scenario.steps,
                difficulty: scenario.difficulty,
                history,
            });

            if (!res.success || !res.data) {
                setError(res.message ?? 'Failed to get response');
                return;
            }

            setChatEntries((prev) => [...prev, { line: res.data! }]);
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to send message');
        } finally {
            setIsSending(false);
        }
    };

    const handleKeyDown = (e: React.KeyboardEvent) => {
        if (e.key === 'Enter' && !e.shiftKey) {
            e.preventDefault();
            handleSend();
        }
    };

    const getAiField = (entry: ChatEntry, field: string): unknown => {
        return (entry.line as Record<string, unknown>)[field];
    };

    if (phase === 'error' && error) {
        return (
            <div className={'session-overlay'}>
                <div className={'session-error'}>
                    <div className={'error-text'}>{error}</div>
                    <button onClick={onClose}>Close</button>
                </div>
            </div>
        );
    }

    if (phase === 'pick-difficulty') {
        return (
            <div className={'session-overlay'}>
                <div className={'difficulty-picker'}>
                    <button className={'close-x'} onClick={onClose}>✕</button>
                    <h2>Choose difficulty</h2>
                    <div className={'difficulty-options'}>
                        {DIFFICULTIES.map((d) => (
                            <button
                                key={d.value}
                                className={'difficulty-btn'}
                                onClick={() => handleStartSession(d.value)}
                                disabled={isSending}
                            >
                                <span className={'diff-label'}>{d.label}</span>
                                <span className={'diff-desc'}>{d.desc}</span>
                            </button>
                        ))}
                    </div>
                    {isSending && <Spinner />}
                </div>
            </div>
        );
    }

    return (
        <div className={'session-overlay'}>
            <div className={'chat-container'}>
                <div className={'chat-header'}>
                    <button className={'close-btn'} onClick={onClose}>✕</button>
                    <div className={'chat-title'}>{scenario?.title ?? 'Scenario'}</div>
                    <div className={'chat-difficulty'}>
                        {DIFFICULTIES.find((d) => d.value === scenario?.difficulty)?.label ?? 'Medium'}
                    </div>
                </div>

                <div className={'chat-messages'}>
                    {chatEntries.length === 0 && (
                        <div className={'chat-empty'}>
                            <div className={'empty-text'}>
                                Start the conversation. Type something in the learning language.
                            </div>
                            <div className={'scenario-steps'}>
                                <strong>Scenario steps:</strong>
                                {scenario?.steps.split('\n').filter(Boolean).map((step, i) => (
                                    <div key={i} className={'step-line'}>{i + 1}. {step}</div>
                                ))}
                            </div>
                        </div>
                    )}

                    {chatEntries.map((entry, i) => (
                        <div key={i} className={`chat-bubble ${entry.line.isUser ? 'user' : 'assistant'}`}>
                            <div className={'bubble-message'}>
                                {entry.line.isUser
                                    ? entry.line.original
                                    : entry.line.message
                                }
                            </div>

                            {!entry.line.isUser && (
                                <>
                                    <div className={'line-actions'}>
                                        <button
                                            className={`line-action-btn${openMenuIndex === i ? ' active' : ''}`}
                                            onClick={(e) => {
                                                e.stopPropagation();
                                                setOpenMenuIndex((prev) => prev === i ? null : i);
                                            }}
                                            aria-label={'Actions'}
                                        >
                                            <svg width="14" height="14" viewBox="0 0 14 14" fill="none">
                                                <path d="M7 3V3.01M7 7V7.01M7 11V11.01" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
                                            </svg>
                                        </button>
                                        {openMenuIndex === i && (
                                            <div className={'line-dropdown'}>
                                                <button
                                                    className={'line-dropdown-item'}
                                                    onClick={(e) => {
                                                        e.stopPropagation();
                                                        setOpenMenuIndex(null);
                                                        setFlashcardIndex(i);
                                                    }}
                                                >
                                                    <svg width="14" height="14" viewBox="0 0 14 14" fill="none">
                                                        <rect x="1" y="3" width="12" height="10" rx="1.5" stroke="currentColor" strokeWidth="1.3"/>
                                                        <path d="M4 1V3M10 1V3M1 6H13" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round"/>
                                                    </svg>
                                                    Create Flash Card
                                                </button>
                                                {i > 0 && chatEntries[i - 1]?.line.isUser && (
                                                    <button
                                                        className={'line-dropdown-item'}
                                                        onClick={(e) => {
                                                            e.stopPropagation();
                                                            setOpenMenuIndex(null);
                                                            setCorrectionIndex(i);
                                                        }}
                                                    >
                                                        <svg width="14" height="14" viewBox="0 0 14 14" fill="none">
                                                            <path d="M2 7L5 10L12 3" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
                                                        </svg>
                                                        Create Correction Flash Card
                                                    </button>
                                                )}
                                            </div>
                                        )}
                                    </div>

                                    {(getAiField(entry, 'correct') !== undefined) && (
                                        <div className={`bubble-evaluation ${getAiField(entry, 'correct') ? 'correct' : 'incorrect'}`}>
                                            <span className={'eval-badge'}>
                                                {getAiField(entry, 'correct') ? '✓ Correct' : '✗ Needs improvement'}
                                            </span>
                                        </div>
                                    )}

                                    <div className={'bubble-details'}>
                                        {getAiField(entry, 'feedback') && (
                                            <div className={'detail-row feedback'}>
                                                <span className={'detail-label'}>Feedback:</span>
                                                <span>{getAiField(entry, 'feedback') as string}</span>
                                            </div>
                                        )}
                                        {entry.line.translation && (
                                            <div className={'detail-row'}>
                                                <span className={'detail-label'}>Translation:</span>
                                                <span>{entry.line.translation}</span>
                                            </div>
                                        )}
                                        {entry.line.pronunciation && (
                                            <div className={'detail-row pronunciation'}>
                                                <span className={'detail-label'}>Pronunciation:</span>
                                                <span>{entry.line.pronunciation}</span>
                                            </div>
                                        )}
                                        {entry.line.culturalMeaning && (
                                            <div className={'detail-row cultural'}>
                                                <span className={'detail-label'}>Culture:</span>
                                                <span>{entry.line.culturalMeaning}</span>
                                            </div>
                                        )}
                                        {entry.line.hint && (
                                            <div className={'detail-row hint'}>
                                                <span className={'detail-label'}>Tip:</span>
                                                <span>{entry.line.hint}</span>
                                            </div>
                                        )}
                                    </div>
                                </>
                            )}
                        </div>
                    ))}

                    {isSending && (
                        <div className={'chat-bubble assistant sending'}>
                            <Spinner />
                        </div>
                    )}

                    <div ref={messagesEndRef} />
                </div>

                {error && (
                    <div className={'chat-error'}>{error}</div>
                )}

                <div className={'chat-input-area'}>
                    <input
                        ref={inputRef}
                        type={'text'}
                        value={userInput}
                        onChange={(e) => setUserInput(e.target.value)}
                        onKeyDown={handleKeyDown}
                        placeholder={'Type your message...'}
                        disabled={isSending}
                    />
                    <button
                        className={'send-btn'}
                        onClick={handleSend}
                        disabled={!userInput.trim() || isSending}
                    >
                        Send
                    </button>
                </div>
            </div>

            {flashcardIndex !== null && chatEntries[flashcardIndex] && (
                <CreateFlashCardModal
                    userId={session?.user?.id ?? ''}
                    languageId={language?.id ?? ''}
                    showLanguageSelector={true}
                    initialValues={{
                        frontStatement: chatEntries[flashcardIndex].line.original,
                        backStatement: chatEntries[flashcardIndex].line.message,
                        pronunciation: chatEntries[flashcardIndex].line.pronunciation,
                        notes: chatEntries[flashcardIndex].line.culturalMeaning || `From scenario: ${scenario?.title ?? 'Unknown'}`,
                        category: 'Scenario',
                        tags: 'scenario',
                    }}
                    onClose={() => setFlashcardIndex(null)}
                    onCreated={() => setFlashcardIndex(null)}
                />
            )}

            {correctionIndex !== null && chatEntries[correctionIndex] && (() => {
                const userEntry = chatEntries[correctionIndex - 1];
                const aiLine = chatEntries[correctionIndex].line as Record<string, unknown>;
                return (
                    <CreateFlashCardModal
                        userId={session?.user?.id ?? ''}
                        languageId={language?.id ?? ''}
                        showLanguageSelector={true}
                        initialValues={{
                            frontStatement: userEntry?.line?.original ?? '',
                            backStatement: (aiLine.hint as string) || '',
                            pronunciation: '',
                            notes: (aiLine.feedback as string) || '',
                            category: 'Scenario-Correction',
                            tags: 'scenario,correction',
                        }}
                        onClose={() => setCorrectionIndex(null)}
                        onCreated={() => setCorrectionIndex(null)}
                    />
                );
            })()}
        </div>
    );
};
