export interface StartConversationRequest {
    profileId: string;
}

export interface ChatMessageDto {
    id: string;
    role: string;
    content: string;
    createdAt: string;
}

export interface StartConversationResponse {
    conversationId: string;
    profileId: string;
    profileName: string;
    profileAvatarUrl: string;
    greeting: ChatMessageDto;
}

export interface ApiResponse<T> {
    success: boolean;
    message?: string | null;
    data?: T | null;
}

export const startConversation = (payload: StartConversationRequest): Promise<ApiResponse<StartConversationResponse>> => {
    return fetch('/api/lemessage/chat/start', {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(payload)
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export interface SendMessageRequest {
    conversationId: string;
    message: string;
}

export interface Correction {
    mistake: string;
    corrected: string;
    explanation: string;
}

export interface SendMessageResponse {
    userMessage: ChatMessageDto;
    assistantMessage: ChatMessageDto;
    correction?: Correction | null;
}

export const sendMessage = (payload: SendMessageRequest): Promise<ApiResponse<SendMessageResponse>> => {
    return fetch('/api/lemessage/chat/send', {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(payload)
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export interface ConversationSummary {
    id: string;
    profileId: string;
    profileName: string;
    profileAvatarUrl: string;
    lastMessage: string;
    lastMessageTime: string;
    createTime: string;
}

export interface ListConversationsResponse {
    conversations: ConversationSummary[];
}

export const listConversations = (): Promise<ApiResponse<ListConversationsResponse>> => {
    return fetch('/api/lemessage/chat/conversations', {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export interface GetMessagesRequest {
    conversationId: string;
    page: number;
    limit: number;
}

export interface GetMessagesResponse {
    messages: ChatMessageDto[];
    totalCount: number;
}

export const getMessages = (payload: GetMessagesRequest): Promise<ApiResponse<GetMessagesResponse>> => {
    return fetch('/api/lemessage/chat/messages', {
        method: "POST",
        headers: {
            "Content-Type": "application/json"
        },
        body: JSON.stringify(payload)
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

