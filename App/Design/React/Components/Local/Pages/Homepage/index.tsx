import { useEffect, useRef, useState } from "react";
import { sendMessage } from '@api/homeChat';
import { Spinner } from '@component/Spinner';
import './style.scss';

interface ChatMessage {
    role: 'user' | 'assistant';
    content: string;
    translation?: string;
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
    const [messages, setMessages] = useState<ChatMessage[]>([]);
    const [userInput, setUserInput] = useState('');
    const [isSending, setIsSending] = useState(false);
    const [error, setError] = useState<string | null>(null);

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

    return (
        <div className={'homepage'}>
            <div className={'homepage__chat'}>
                <div className={'homepage__chat-messages'}>
                    {!hasMessages && (
                        <div className={'homepage__empty'}>
                            <h1 className={'homepage__title'}>Welcome to LLE</h1>
                            <p className={'homepage__subtitle'}>Choose a learning activity below or start a free-form conversation.</p>
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
        </div>
    );
};
