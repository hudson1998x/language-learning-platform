export interface ChatMessage {
    conversationId: string;
    role: string;
    content: string;
    id: string;
    createTime: string;
    updateTime: string;
}

export interface ApiResponse<T> {
    success: boolean;
    message?: string | null;
    data?: T | null;
}

export const createChatMessage = (payload: ChatMessage): Promise<ApiResponse<ChatMessage>> => {
    return fetch('/api/chatmessage/create', {
        method: "PUT",
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

export const updateChatMessage = (payload: ChatMessage): Promise<ApiResponse<ChatMessage>> => {
    return fetch('/api/chatmessage/update', {
        method: "PATCH",
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

export const deleteChatMessage = (payload: ChatMessage): Promise<ApiResponse<ChatMessage>> => {
    return fetch('/api/chatmessage/delete', {
        method: "DELETE",
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

export const deleteChatMessageById = (id: string): Promise<ApiResponse<ChatMessage>> => {
    return fetch(`/api/chatmessage/deleteById/${id}`, {
        method: "DELETE",
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

export const listAllChatMessage = (): Promise<ApiResponse<ChatMessage[]>> => {
    return fetch('/api/chatmessage/list', {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const listChatMessagePaged = (pageNum: string, size: string): Promise<ApiResponse<ChatMessage[]>> => {
    return fetch(`/api/chatmessage/list/${pageNum}/${size}`, {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const listChatMessagePagedSorted = (pageNum: string, size: string, sortField: string, sortDir: string): Promise<ApiResponse<ChatMessage[]>> => {
    return fetch(`/api/chatmessage/list/${pageNum}/${size}/${sortField}/${sortDir}`, {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const loadChatMessage = (id: string): Promise<ApiResponse<ChatMessage>> => {
    return fetch(`/api/chatmessage/${id}`, {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

