import { useState, useCallback } from "react";
import { register } from "@registry";
import { ConversationList } from './ConversationList';
import { ChatView } from './ChatView';
import './style.scss';

interface ConvInfo {
    profileName: string;
    profileAvatarUrl: string;
}

const LeMessagePage = () => {
    const [activeConvId, setActiveConvId] = useState<string | null>(null);
    const [activeConvInfo, setActiveConvInfo] = useState<ConvInfo>({ profileName: '', profileAvatarUrl: '' });
    const [refreshTrigger, setRefreshTrigger] = useState(0);

    const handleSelectConversation = useCallback((id: string, profileName: string, profileAvatarUrl: string) => {
        setActiveConvId(id);
        setActiveConvInfo({ profileName, profileAvatarUrl });
    }, []);

    const handleConversationCreated = useCallback((id: string, profileName: string, profileAvatarUrl: string) => {
        setActiveConvId(id);
        setActiveConvInfo({ profileName, profileAvatarUrl });
        setRefreshTrigger((prev) => prev + 1);
    }, []);

    const handleMessageSent = useCallback(() => {
        setRefreshTrigger((prev) => prev + 1);
    }, []);

    const handleBack = useCallback(() => {
        setActiveConvId(null);
        setActiveConvInfo({ profileName: '', profileAvatarUrl: '' });
    }, []);

    return (
        <div className={'lemessage-page'}>
            <ConversationList
                activeConversationId={activeConvId}
                onSelectConversation={handleSelectConversation}
                onConversationCreated={handleConversationCreated}
                refreshTrigger={refreshTrigger}
            />
            <ChatView
                conversationId={activeConvId}
                profileName={activeConvInfo.profileName}
                profileAvatarUrl={activeConvInfo.profileAvatarUrl}
                onBack={handleBack}
                onMessageSent={handleMessageSent}
            />
        </div>
    );
};

register('@page/lemessage-chat', LeMessagePage);
