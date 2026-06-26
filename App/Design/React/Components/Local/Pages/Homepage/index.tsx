import { useEffect, useRef, useState } from "react";
import { sendMessage } from '@api/homeChat';
import { Spinner } from '@component/Spinner';
import { CreateFlashCardModal } from '@component/Pages/FlashCards/CreateFlashCardModal';
import { useSession } from '@hook/session-provider';
import { useLanguage } from '@hook/language-provider';
import {useLlmCapability} from '@hook/llm-capability-provider'
import './style.scss';

interface ChatMessage {
    role: 'user' | 'assistant';
    content: string;
    translation?: string;
    pronunciation?: string;
}

interface Tile {
    label: string;
    href: string;
    icon: string;
    subtitle: string;
}

const TILES: Tile[] = [
    {
        label: 'Flash cards',
        href: '/flashcards',
        icon: 'cards',
        subtitle: 'Review vocabulary with spaced repetition',
    },
    {
        label: 'Music translation',
        href: '/musiclyrics',
        icon: 'music',
        subtitle: 'Translate song lyrics in real time',
    },
    {
        label: 'Scenarios',
        href: '/scenarios',
        icon: 'scenes',
        subtitle: 'Practice real-life conversations',
    },
    {
        label: 'LeMessage',
        href: '/messages',
        icon: 'chat',
        subtitle: 'Chat with AI language partners',
    },
];

const TileIcon = ({ icon }: { icon: string }) => {
    switch (icon) {
        case 'cards':
            return (
                <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
                    <rect x="3" y="3" width="18" height="18" rx="2" />
                    <path d="M9 8h6M9 12h6M9 16h4" />
                </svg>
            );
        case 'music':
            return (
                <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M9 18V5l12-2v13" />
                    <circle cx="6" cy="18" r="3" />
                    <circle cx="18" cy="16" r="3" />
                </svg>
            );
        case 'scenes':
            return (
                <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
                    <rect x="2" y="3" width="20" height="14" rx="2" />
                    <path d="M8 21h8M12 17v4" />
                </svg>
            );
        case 'chat':
            return (
                <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
                    <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z" />
                </svg>
            );
        default:
            return null;
    }
};

export const Homepage = () => {
    const { session } = useSession();
    const { language } = useLanguage();
    const { available: llmIsAvailable } = useLlmCapability();

    const [messages, setMessages] = useState<ChatMessage[]>([]);
    const [userInput, setUserInput] = useState('');
    const [isSending, setIsSending] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [flashcardIndex, setFlashcardIndex] = useState<number | null>(null);

    const messagesEndRef = useRef<HTMLDivElement>(null);
    const inputRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [messages]);

    useEffect(() => {
        if (!isSending) {
            inputRef.current?.focus();
        }
    }, [isSending]);

    const handleSend = async () => {
        if (!userInput.trim() || isSending) return;

        const text = userInput;
        setUserInput('');
        setIsSending(true);
        setError(null);

        const userMsg: ChatMessage = { role: 'user', content: text };
        setMessages((prev) => [...prev, userMsg]);

        try {
            const history = messages.map((m) => ({
                role: m.role,
                content: m.content,
            }));

            const res = await sendMessage({ message: text, history });

            if (res.success && res.data) {
                setMessages((prev) => [
                    ...prev,
                    {
                        role: 'assistant',
                        content: res.data!.reply,
                        translation: res.data!.translation,
                        pronunciation: res.data!.pronunciation,
                    },
                ]);
            } else {
                setError(res.message ?? 'Failed to get response');
                setMessages((prev) => prev.filter((m) => m !== userMsg));
            }
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to send message');
            setMessages((prev) => prev.filter((m) => m !== userMsg));
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

    const hasMessages = messages.length > 0;
    
    if (!llmIsAvailable)
    {
        return (
            <div className={'homepage'}>
                <p className={'warn'}>This page requires an LLM, please head to your <a href={'/settings'}>Settings</a> to connect one.</p>
            </div>
        )
    }

    return (
        <div className={'homepage'}>
            <div className={'homepage__chat'}>
                <div className={'homepage__chat-messages'}>
                    {!hasMessages && (
                        <div className={'homepage__empty'}>
                            <h1 className={'homepage__title'}>What would you like to learn today?</h1>
                            <p className={'homepage__subtitle'}>Practice a conversation, translate music,
                                review flashcards, or simply ask for help.</p>
                            <div className={'homepage__tiles'}>
                                {TILES.map((tile) => (
                                    <a key={tile.href} href={tile.href} className={'homepage__tile'}>
                                        <div className={'homepage__tile-icon'}>
                                            <TileIcon icon={tile.icon} />
                                        </div>
                                        <div className={'homepage__tile-label'}>{tile.label}</div>
                                        <div className={'homepage__tile-subtitle'}>{tile.subtitle}</div>
                                    </a>
                                ))}
                            </div>
                        </div>
                    )}

                    {hasMessages && messages.map((msg, i) => (
                        <div
                            key={i}
                            className={`homepage__bubble ${msg.role === 'user' ? 'user' : 'assistant'}`}
                        >
                            <div className={'homepage__bubble-content'}>
                                {msg.content}
                            </div>
                            {msg.role === 'assistant' && msg.translation && (
                                <div className={'homepage__bubble-translation'}>
                                    {msg.translation}
                                </div>
                            )}
                            {msg.role === 'assistant' && (
                                <div className={'homepage__bubble-actions'}>
                                    <button
                                        className={'homepage__card-btn'}
                                        onClick={() => setFlashcardIndex(i)}
                                        title={'Create flash card'}
                                    >
                                        <svg width="14" height="14" viewBox="0 0 14 14" fill="none">
                                            <rect x="1" y="3" width="12" height="10" rx="1.5" stroke="currentColor" strokeWidth="1.3" />
                                            <path d="M4 1V3M10 1V3M1 6H13" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round" />
                                        </svg>
                                        Flash card
                                    </button>
                                </div>
                            )}
                        </div>
                    ))}

                    {isSending && (
                        <div className={'homepage__bubble assistant sending'}>
                            <Spinner size={'sm'} />
                        </div>
                    )}

                    <div ref={messagesEndRef} />
                </div>

                {error && (
                    <div className={'homepage__chat-error'}>{error}</div>
                )}

                <div className={'homepage__chat-input'}>
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
                        className={'homepage__send-btn'}
                        onClick={handleSend}
                        disabled={!userInput.trim() || isSending}
                    >
                        <svg width="18" height="18" viewBox="0 0 18 18" fill="none">
                            <path d="M2 9L16 2L9 16L7 11L2 9Z" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
                        </svg>
                    </button>
                </div>
            </div>

            {flashcardIndex !== null && messages[flashcardIndex] && (() => {
                const msg = messages[flashcardIndex];
                const prevMsg = flashcardIndex > 0 ? messages[flashcardIndex - 1] : null;
                return (
                    <CreateFlashCardModal
                        userId={session?.user?.id ?? ''}
                        languageId={language?.id ?? ''}
                        showLanguageSelector={true}
                        initialValues={{
                            frontStatement: msg.content,
                            backStatement: msg.translation ?? '',
                            pronunciation: msg.pronunciation ?? '',
                            notes: `From home chat conversation`,
                            category: 'Home-Chat',
                            tags: 'homechat',
                        }}
                        onClose={() => setFlashcardIndex(null)}
                        onCreated={() => setFlashcardIndex(null)}
                    />
                );
            })()}
        </div>
    );
};
