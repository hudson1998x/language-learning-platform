import { useEffect, useState } from "react";
import { useSession } from '@hook/session-provider';
import { Spinner } from '@component/Spinner';
import { listAllProfile, Profile } from '@api/profile';
import { startConversation } from '@api/leMessage';
import { useLanguage } from '@hook/language-provider';
import './ConversationList.scss';

interface ConversationSummary {
    id: string;
    profileId: string;
    profileName: string;
    profileAvatarUrl: string;
    lastMessage: string;
    lastMessageTime: string;
    createTime: string;
}

interface ConversationListProps {
    activeConversationId: string | null;
    onSelectConversation: (id: string) => void;
    onConversationCreated: (id: string) => void;
    refreshTrigger: number;
}

export const ConversationList = ({
    activeConversationId,
    onSelectConversation,
    onConversationCreated,
    refreshTrigger
}: ConversationListProps) => {
    const { session } = useSession();
    const { language } = useLanguage();

    const [conversations, setConversations] = useState<ConversationSummary[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [showProfilePicker, setShowProfilePicker] = useState(false);

    useEffect(() => {
        loadConversations();
    }, [session?.user?.id, refreshTrigger]);

    const loadConversations = async () => {
        if (!session?.user?.id) return;
        setIsLoading(true);
        setError(null);

        try {
            const res = await fetch('/api/lemessage/chat/conversations', { method: "GET" }).then(r => r.json());
            if (res.success && res.data) {
                setConversations(res.data.conversations ?? []);
            } else {
                setConversations([]);
            }
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to load conversations');
        } finally {
            setIsLoading(false);
        }
    };

    const formatTime = (iso?: string): string => {
        if (!iso) return '';
        const date = new Date(iso);
        const now = Date.now();
        const diff = now - date.getTime();
        const days = Math.floor(diff / 86_400_000);
        if (days <= 0) {
            const hours = Math.floor(diff / 3_600_000);
            if (hours <= 0) return 'Just now';
            return `${hours}h ago`;
        }
        if (days === 1) return 'Yesterday';
        if (days < 7) return `${days}d ago`;
        if (days < 30) return `${Math.floor(days / 7)}w ago`;
        return date.toLocaleDateString();
    };

    if (isLoading) {
        return (
            <div className={'conv-list'}>
                <div className={'conv-list-header'}>
                    <h2>Messages</h2>
                </div>
                <div className={'conv-list-status'}><Spinner /></div>
            </div>
        );
    }

    if (error) {
        return (
            <div className={'conv-list'}>
                <div className={'conv-list-header'}>
                    <h2>Messages</h2>
                </div>
                <div className={'conv-list-status'}>
                    <div className={'error-text'}>{error}</div>
                    <button className={'retry-btn'} onClick={loadConversations}>Retry</button>
                </div>
            </div>
        );
    }

    return (
        <div className={'conv-list'}>
            <div className={'conv-list-header'}>
                <h2>Messages</h2>
                <button
                    className={'new-chat-btn'}
                    onClick={() => setShowProfilePicker(true)}
                    title={'New conversation'}
                >
                    <svg width="18" height="18" viewBox="0 0 18 18" fill="none">
                        <path d="M9 3V15M3 9H15" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
                    </svg>
                </button>
            </div>

            <div className={'conv-list-scroll'}>
                {conversations.length === 0 && (
                    <div className={'conv-list-empty'}>
                        <div className={'empty-icon'}>
                            <svg width="40" height="40" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round">
                                <path d="M21 15a2 2 0 0 1-2 2H7l-4 4V5a2 2 0 0 1 2-2h14a2 2 0 0 1 2 2z"/>
                            </svg>
                        </div>
                        <div className={'empty-text'}>No conversations yet</div>
                        <div className={'empty-hint'}>Start a new chat to begin practising!</div>
                    </div>
                )}

                {conversations.map((conv) => (
                    <div
                        key={conv.id}
                        className={`conv-item ${activeConversationId === conv.id ? 'active' : ''}`}
                        onClick={() => onSelectConversation(conv.id)}
                        role={'button'}
                        tabIndex={0}
                        onKeyDown={(e) => {
                            if (e.key === 'Enter') onSelectConversation(conv.id);
                        }}
                    >
                        <div className={'conv-avatar'}>
                            {conv.profileAvatarUrl ? (
                                <img src={conv.profileAvatarUrl} alt={conv.profileName} />
                            ) : (
                                <div className={'avatar-placeholder'}>
                                    {conv.profileName.charAt(0)}
                                </div>
                            )}
                        </div>
                        <div className={'conv-info'}>
                            <div className={'conv-name-row'}>
                                <span className={'conv-name'}>{conv.profileName}</span>
                                <span className={'conv-time'}>{formatTime(conv.lastMessageTime)}</span>
                            </div>
                            <div className={'conv-preview'}>
                                {conv.lastMessage || 'No messages yet'}
                            </div>
                        </div>
                    </div>
                ))}
            </div>

            {showProfilePicker && (
                <div className={'profile-picker-overlay'} onClick={() => setShowProfilePicker(false)}>
                    <div className={'profile-picker'} onClick={(e) => e.stopPropagation()}>
                        <div className={'profile-picker-header'}>
                            <h3>New Conversation</h3>
                            <button className={'modal-close'} onClick={() => setShowProfilePicker(false)}>✕</button>
                        </div>
                        <ProfilePickerContent
                            language={language}
                            userId={session?.user?.id ?? ''}
                            languageId={language?.id ?? ''}
                            onSelected={(convId) => {
                                setShowProfilePicker(false);
                                onConversationCreated(convId);
                            }}
                        />
                    </div>
                </div>
            )}
        </div>
    );
};

interface ProfilePickerContentProps {
    language: { id: string; name: string } | undefined;
    userId: string;
    languageId: string;
    onSelected: (conversationId: string) => void;
}

const ProfilePickerContent = ({ language, userId, languageId, onSelected }: ProfilePickerContentProps) => {
    const [profiles, setProfiles] = useState<Profile[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [startingId, setStartingId] = useState<string | null>(null);

    useEffect(() => {
        loadProfiles();
    }, [language]);

    const loadProfiles = async () => {
        setIsLoading(true);
        setError(null);
        try {
            const res = await listAllProfile();
            if (res.success && res.data) {
                const filtered = language?.name
                    ? res.data.filter(p => p.languageName === language.name)
                    : res.data;
                setProfiles(filtered);
            } else {
                setError(res.message ?? 'Failed to load profiles');
            }
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to load profiles');
        } finally {
            setIsLoading(false);
        }
    };

    const handleStart = async (profileId: string) => {
        setStartingId(profileId);
        try {
            const res = await startConversation({ profileId });
            if (res.success && res.data) {
                onSelected(res.data.conversationId);
            } else {
                setError(res.message ?? 'Failed to start conversation');
            }
        } catch (err) {
            setError(err instanceof Error ? err.message : 'Failed to start conversation');
        } finally {
            setStartingId(null);
        }
    };

    if (isLoading) {
        return (
            <div className={'profile-picker-status'}>
                <Spinner />
            </div>
        );
    }

    if (error) {
        return (
            <div className={'profile-picker-status'}>
                <div className={'error-text'}>{error}</div>
                <button className={'retry-btn'} onClick={loadProfiles}>Retry</button>
            </div>
        );
    }

    if (profiles.length === 0) {
        return (
            <div className={'profile-picker-status'}>
                <div className={'empty-text'}>No profiles available{language?.name ? ` for ${language.name}` : ''}</div>
            </div>
        );
    }

    return (
        <div className={'profile-grid'}>
            {profiles.map((profile) => (
                <button
                    key={profile.id}
                    className={'profile-card'}
                    onClick={() => handleStart(profile.id)}
                    disabled={startingId === profile.id}
                >
                    <div className={'profile-card-avatar'}>
                        {profile.avatarUrl ? (
                            <img src={profile.avatarUrl} alt={profile.name} />
                        ) : (
                            <div className={'avatar-placeholder'}>{profile.name.charAt(0)}</div>
                        )}
                    </div>
                    <div className={'profile-card-info'}>
                        <div className={'profile-card-name'}>{profile.name}</div>
                        <div className={'profile-card-desc'}>{profile.description}</div>
                    </div>
                    {startingId === profile.id && <Spinner size={'sm'} />}
                </button>
            ))}
        </div>
    );
};
