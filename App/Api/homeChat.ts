export interface HomeChatHistoryEntry {
    role: string;
    content: string;
}

export interface SendMessageRequest {
    message: string;
    history?: HomeChatHistoryEntry[] | null;
}

export interface HomeChatResponse {
    reply: string;
    translation: string;
    pronunciation: string;
}

export interface ApiResponse<T> {
    success: boolean;
    message?: string | null;
    data?: T | null;
}

export const sendMessage = (payload: SendMessageRequest): Promise<ApiResponse<HomeChatResponse>> => {
    return fetch('/api/homechat/send', {
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

export interface PronounceRequest {
    text: string;
}

export interface PronounceResponse {
    pronunciation: string;
}

export const generatePronunciation = (payload: PronounceRequest): Promise<ApiResponse<PronounceResponse>> => {
    return fetch('/api/homechat/pronounce', {
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
