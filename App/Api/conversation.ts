export interface Conversation {
    profileId: string;
    userId: string;
    title: string;
    id: string;
    createTime: string;
    updateTime: string;
}

export interface ApiResponse<T> {
    success: boolean;
    message?: string | null;
    data?: T | null;
}

export const createConversation = (payload: Conversation): Promise<ApiResponse<Conversation>> => {
    return fetch('/api/conversation/create', {
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

export const updateConversation = (payload: Conversation): Promise<ApiResponse<Conversation>> => {
    return fetch('/api/conversation/update', {
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

export const deleteConversation = (payload: Conversation): Promise<ApiResponse<Conversation>> => {
    return fetch('/api/conversation/delete', {
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

export const deleteConversationById = (id: string): Promise<ApiResponse<Conversation>> => {
    return fetch(`/api/conversation/deleteById/${id}`, {
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

export const listAllConversation = (): Promise<ApiResponse<Conversation[]>> => {
    return fetch('/api/conversation/list', {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const listConversationPaged = (pageNum: string, size: string): Promise<ApiResponse<Conversation[]>> => {
    return fetch(`/api/conversation/list/${pageNum}/${size}`, {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const listConversationPagedSorted = (pageNum: string, size: string, sortField: string, sortDir: string): Promise<ApiResponse<Conversation[]>> => {
    return fetch(`/api/conversation/list/${pageNum}/${size}/${sortField}/${sortDir}`, {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const loadConversation = (id: string): Promise<ApiResponse<Conversation>> => {
    return fetch(`/api/conversation/${id}`, {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

