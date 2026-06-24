import { useEffect, useRef, useState } from "react";
import { getMessages, ChatMessageDto } from '@api/leMessage';
import { Spinner } from '@component/Spinner';
import { useSession } from '@hook/session-provider';
import { useLanguage } from '@hook/language-provider';
import { CreateFlashCardModal } from '@component/Pages/FlashCards/CreateFlashCardModal';
import './ChatView.scss';

interface Correction {
    mistake: string;
    corrected: string;
    explanation: string;
}

interface SendMessageResponse {
    userMessage: ChatMessageDto;
    assistantMessage: ChatMessageDto;
    correction?: Correction | null;
}

interface ChatViewProps {
    conversationId: string | null;
    profileName: string;
    profileAvatarUrl: string;
    onBack: () => void;
    onMessageSent: () => void;
}

export const ChatView = ({
    conversationId,
    profileName,
    profileAvatarUrl,
    onBack,
    onMessageSent
}: ChatViewProps) => {
    const { session } = useSession();
    const { language } = useLanguage();

    const [messages, setMessages] = useState<ChatMessageDto[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [isSending, setIsSending] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [userInput, setUserInput] = useState('');
    const [correctionData, setCorrectionData] = useState<Correction | null>(null);

    const messagesEndRef = useRef<HTMLDivElement>(null);
    const inputRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        if (conversationId) {
            loadMessages();
        } else {
            setMessages([]);
        }
    }, [conversationId]);

    useEffect(() => {
        messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [messages]);

    useEffect(() => {
        if (!isSending && conversationId) {
            inputRef.current?.focus();
        }
    }, [isSending, conversationId]);

    const loadMessages = async () => {
        if (!conversationId) return;
        setIsLoading(true);
        setError(null);
        try {
            const res = await getMessages({ conversationId, page: 1, limit: 100 });
            if (res.success && res.data) {
                setMessages(res.data.messages ?? []);
            } else {
                setError(res.message ?? 'Failed to load messages');
            }
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to load messages');
        } finally {
            setIsLoading(false);
        }
    };

    const handleSend = async () => {
        if (!userInput.trim() || !conversationId || isSending) return;

        const text = userInput;
        setUserInput('');
        setIsSending(true);
        setError(null);

        const optimisticMsg: ChatMessageDto = {
            id: 'temp-' + Date.now(),
            role: 'user',
            content: text,
            createdAt: new Date().toISOString(),
        };
        setMessages((prev) => [...prev, optimisticMsg]);

        try {
            const raw = await fetch('/api/lemessage/chat/send', {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ conversationId, message: text })
            }).then(r => r.json());

            if (raw.success && raw.data) {
                setMessages((prev) =>
                    prev.filter(m => m.id !== optimisticMsg.id)
                        .concat(raw.data.userMessage)
                        .concat(raw.data.assistantMessage)
                );
                if (raw.data.correction) {
                    setCorrectionData(raw.data.correction);
                }
                onMessageSent();
            } else {
                setMessages((prev) => prev.filter(m => m.id !== optimisticMsg.id));
                setError(raw.message ?? 'Failed to get response');
            }
        } catch (err) {
            setMessages((prev) => prev.filter(m => m.id !== optimisticMsg.id));
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

    const formatTime = (iso?: string): string => {
        if (!iso) return '';
        const date = new Date(iso);
        const now = Date.now();
        const diff = now - date.getTime();
        if (diff < 60_000) return 'Just now';
        const mins = Math.floor(diff / 60_000);
        if (mins < 60) return `${mins}m ago`;
        const hours = Math.floor(diff / 3_600_000);
        if (hours < 24) return `${hours}h ago`;
        return date.toLocaleDateString();
    };

    if (!conversationId) {
        return (
            <div className={'chat-view'}>
                <div className={'chat-view-empty'}>
                    <div className={'empty-icon'}>
                        <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
                            <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>
                        </svg>
                    </div>
                    <div className={'empty-title'}>Select a conversation</div>
                    <div className={'empty-hint'}>Choose a chat from the sidebar or start a new one</div>
                </div>
            </div>
        );
    }

    return (
        <div className={'chat-view'}>
            <div className={'chat-view-header'}>
                <button className={'back-btn'} onClick={onBack} aria-label={'Back'}>
                    <svg width="20" height="20" viewBox="0 0 20 20" fill="none">
                        <path d="M12 4L6 10L12 16" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round"/>
                    </svg>
                </button>
                <div className={'chat-view-profile'}>
                    <div className={'header-avatar'}>
                        {profileAvatarUrl ? (
                            <img src={profileAvatarUrl} alt={profileName} />
                        ) : (
                            <div className={'avatar-placeholder'}>{profileName.charAt(0)}</div>
                        )}
                    </div>
                    <div className={'header-name'}>{profileName}</div>
                </div>
            </div>

            <div className={'chat-view-messages'}>
                {isLoading && (
                    <div className={'chat-view-status'}><Spinner /></div>
                )}

                {!isLoading && messages.length === 0 && (
                    <div className={'chat-view-status'}>
                        <div className={'empty-text'}>Start the conversation!</div>
                        <div className={'empty-hint'}>Say hello to {profileName}</div>
                    </div>
                )}

                {messages.map((msg) => (
                    <div key={msg.id} className={`msg-bubble ${msg.role === 'user' ? 'user' : 'assistant'}`}>
                        <div className={'msg-content'}>{msg.content}</div>
                        <div className={'msg-time'}>{formatTime(msg.createdAt)}</div>
                    </div>
                ))}

                {isSending && (
                    <div className={'msg-bubble assistant sending'}>
                        <Spinner size={'sm'} />
                    </div>
                )}

                <div ref={messagesEndRef} />
            </div>

            {error && (
                <div className={'chat-view-error'}>{error}</div>
            )}

            <div className={'chat-view-input'}>
                <input
                    ref={inputRef}
                    type={'text'}
                    value={userInput}
                    onChange={(e) => setUserInput(e.target.value)}
                    onKeyDown={handleKeyDown}
                    placeholder={`Message ${profileName}...`}
                    disabled={isSending}
                />
                <button
                    className={'send-btn'}
                    onClick={handleSend}
                    disabled={!userInput.trim() || isSending}
                >
                    <svg width="18" height="18" viewBox="0 0 18 18" fill="none">
                        <path d="M2 9L16 2L9 16L7 11L2 9Z" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
                    </svg>
                </button>
            </div>

            {correctionData && (
                <div className={'correction-toast'}>
                    <div className={'correction-toast-content'}>
                        <span className={'correction-label'}>Correction detected!</span>
                        <button
                            className={'correction-card-btn'}
                            onClick={() => setCorrectionData(null)}
                        >
                            Dismiss
                        </button>
                        <CreateFlashCardButton
                            mistake={correctionData.mistake}
                            corrected={correctionData.corrected}
                            explanation={correctionData.explanation}
                            userId={session?.user?.id ?? ''}
                            languageId={language?.id ?? ''}
                            onCreated={() => setCorrectionData(null)}
                        />
                    </div>
                </div>
            )}
        </div>
    );
};

interface CreateFlashCardButtonProps {
    mistake: string;
    corrected: string;
    explanation: string;
    userId: string;
    languageId: string;
    onCreated: () => void;
}

const CreateFlashCardButton = ({ mistake, corrected, explanation, userId, languageId, onCreated }: CreateFlashCardButtonProps) => {
    const [show, setShow] = useState(false);

    return (
        <>
            <button className={'create-card-btn'} onClick={() => setShow(true)}>
                <svg width="14" height="14" viewBox="0 0 14 14" fill="none">
                    <rect x="1" y="3" width="12" height="10" rx="1.5" stroke="currentColor" strokeWidth="1.3"/>
                    <path d="M4 1V3M10 1V3M1 6H13" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round"/>
                </svg>
                Create Flash Card
            </button>
            {show && (
                <CreateFlashCardModal
                    userId={userId}
                    languageId={languageId}
                    showLanguageSelector={true}
                    initialValues={{
                        frontStatement: mistake,
                        backStatement: corrected,
                        notes: explanation,
                        category: 'Chat-Correction',
                        tags: 'lemessage,correction',
                    }}
                    onClose={() => setShow(false)}
                    onCreated={() => {
                        setShow(false);
                        onCreated();
                    }}
                />
            )}
        </>
    );
};
