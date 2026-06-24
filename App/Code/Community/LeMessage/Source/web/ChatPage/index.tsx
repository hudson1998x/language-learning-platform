import { useState, useCallback } from "react";
import { register } from "@registry";
import { ConversationList } from './ConversationList';
import { ChatView } from './ChatView';
import './style.scss';

interface ConversationSummary {
    id: string;
    profileId: string;
    profileName: string;
    profileAvatarUrl: string;
    lastMessage: string;
    lastMessageTime: string;
    createTime: string;
}

const LeMessagePage = () => {
    const [activeConversationId, setActiveConversationId] = useState<string | null>(null);
    const [refreshTrigger, setRefreshTrigger] = useState(0);
    const [convCache, setConvCache] = useState<Map<string, ConversationSummary>>(new Map());

    const handleSelectConversation = useCallback((id: string) => {
        setActiveConversationId(id);
    }, []);

    const handleConversationCreated = useCallback((id: string) => {
        setActiveConversationId(id);
        setRefreshTrigger((prev) => prev + 1);
    }, []);

    const handleMessageSent = useCallback(() => {
        setRefreshTrigger((prev) => prev + 1);
    }, []);

    const handleBack = useCallback(() => {
        setActiveConversationId(null);
    }, []);

    const activeConversation = activeConversationId
        ? convCache.get(activeConversationId)
        : undefined;

    const handleCacheUpdate = useCallback((convs: ConversationSummary[]) => {
        const map = new Map<string, ConversationSummary>();
        convs.forEach((c) => map.set(c.id, c));
        setConvCache(map);
    }, []);

    return (
        <div className={'lemessage-page'}>
            <ConversationList
                activeConversationId={activeConversationId}
                onSelectConversation={handleSelectConversation}
                onConversationCreated={handleConversationCreated}
                refreshTrigger={refreshTrigger}
            />
            <ChatView
                conversationId={activeConversationId}
                profileName={activeConversation?.profileName ?? ''}
                profileAvatarUrl={activeConversation?.profileAvatarUrl ?? ''}
                onBack={handleBack}
                onMessageSent={handleMessageSent}
            />
        </div>
    );
};

register('@page/lemessage-chat', LeMessagePage);
