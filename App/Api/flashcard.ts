export interface FlashCard {
    userId: string;
    languageId: string;
    frontStatement: string;
    backStatement: string;
    pronunciation?: string | null;
    notes?: string | null;
    category?: string | null;
    tags?: string | null;
    difficulty: number;
    lastReviewedUtc?: string | null;
    reviewCount: number;
    correctCount: number;
    incorrectCount: number;
    streak: number;
    id: string;
    createTime: string;
    updateTime: string;
}

export interface ApiResponse<T> {
    success: boolean;
    message?: string | null;
    data?: T | null;
}

export const createFlashCard = (payload: FlashCard): Promise<ApiResponse<FlashCard>> => {
    return fetch('/api/flashcard/create', {
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

export const updateFlashCard = (payload: FlashCard): Promise<ApiResponse<FlashCard>> => {
    return fetch('/api/flashcard/update', {
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

export const deleteFlashCard = (payload: FlashCard): Promise<ApiResponse<FlashCard>> => {
    return fetch('/api/flashcard/delete', {
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

export const deleteFlashCardById = (id: string): Promise<ApiResponse<FlashCard>> => {
    return fetch(`/api/flashcard/deleteById/${id}`, {
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

export const listAllFlashCard = (): Promise<ApiResponse<FlashCard[]>> => {
    return fetch('/api/flashcard/list', {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const listFlashCardPaged = (pageNum: string, size: string): Promise<ApiResponse<FlashCard[]>> => {
    return fetch(`/api/flashcard/list/${pageNum}/${size}`, {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const listFlashCardPagedSorted = (pageNum: string, size: string, sortField: string, sortDir: string): Promise<ApiResponse<FlashCard[]>> => {
    return fetch(`/api/flashcard/list/${pageNum}/${size}/${sortField}/${sortDir}`, {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const loadFlashCard = (id: string): Promise<ApiResponse<FlashCard>> => {
    return fetch(`/api/flashcard/${id}`, {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

