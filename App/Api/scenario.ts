export interface Scenario {
    title: string;
    steps: string;
    id: string;
    createTime: string;
    updateTime: string;
}

export interface ApiResponse<T> {
    success: boolean;
    message?: string | null;
    data?: T | null;
}

export const createScenario = (payload: Scenario): Promise<ApiResponse<Scenario>> => {
    return fetch('/api/scenario/create', {
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

export const updateScenario = (payload: Scenario): Promise<ApiResponse<Scenario>> => {
    return fetch('/api/scenario/update', {
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

export const deleteScenario = (payload: Scenario): Promise<ApiResponse<Scenario>> => {
    return fetch('/api/scenario/delete', {
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

export const deleteScenarioById = (id: string): Promise<ApiResponse<Scenario>> => {
    return fetch(`/api/scenario/deleteById/${id}`, {
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

export const listAllScenario = (): Promise<ApiResponse<Scenario[]>> => {
    return fetch('/api/scenario/list', {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const listScenarioPaged = (pageNum: string, size: string): Promise<ApiResponse<Scenario[]>> => {
    return fetch(`/api/scenario/list/${pageNum}/${size}`, {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const listScenarioPagedSorted = (pageNum: string, size: string, sortField: string, sortDir: string): Promise<ApiResponse<Scenario[]>> => {
    return fetch(`/api/scenario/list/${pageNum}/${size}/${sortField}/${sortDir}`, {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const loadScenario = (id: string): Promise<ApiResponse<Scenario>> => {
    return fetch(`/api/scenario/${id}`, {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export interface StartStudySessionRequest {
    scenarioId: string;
    difficulty: number;
}

export interface StartStudySessionResponse {
    scenarioId: string;
    title: string;
    steps: string;
    difficulty: number;
}

export const startStudySession = (payload: StartStudySessionRequest): Promise<ApiResponse<StartStudySessionResponse>> => {
    return fetch('/api/scenario/studysession/start', {
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

export interface ChatMessage {
    role: string;
    content: string;
}

export interface SendMessageRequest {
    message: string;
    scenarioTitle: string;
    scenarioSteps: string;
    difficulty: number;
    language: string;
    history?: ChatMessage[] | null;
}

export interface ScenarioLine {
    original: string;
    message: string;
    translation: string;
    pronunciation: string;
    culturalMeaning: string;
    hint: string;
    isUser: boolean;
}

export const sendMessage = (payload: SendMessageRequest): Promise<ApiResponse<ScenarioLine>> => {
    return fetch('/api/scenario/studysession/send', {
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

