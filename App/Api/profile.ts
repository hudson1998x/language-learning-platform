export interface Profile {
    name: string;
    avatarUrl: string;
    description: string;
    systemPrompt: string;
    languageName: string;
    id: string;
    createTime: string;
    updateTime: string;
}

export interface ApiResponse<T> {
    success: boolean;
    message?: string | null;
    data?: T | null;
}

export const createProfile = (payload: Profile): Promise<ApiResponse<Profile>> => {
    return fetch('/api/profile/create', {
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

export const updateProfile = (payload: Profile): Promise<ApiResponse<Profile>> => {
    return fetch('/api/profile/update', {
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

export const deleteProfile = (payload: Profile): Promise<ApiResponse<Profile>> => {
    return fetch('/api/profile/delete', {
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

export const deleteProfileById = (id: string): Promise<ApiResponse<Profile>> => {
    return fetch(`/api/profile/deleteById/${id}`, {
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

export const listAllProfile = (): Promise<ApiResponse<Profile[]>> => {
    return fetch('/api/profile/list', {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const listProfilePaged = (pageNum: string, size: string): Promise<ApiResponse<Profile[]>> => {
    return fetch(`/api/profile/list/${pageNum}/${size}`, {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const listProfilePagedSorted = (pageNum: string, size: string, sortField: string, sortDir: string): Promise<ApiResponse<Profile[]>> => {
    return fetch(`/api/profile/list/${pageNum}/${size}/${sortField}/${sortDir}`, {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const loadProfile = (id: string): Promise<ApiResponse<Profile>> => {
    return fetch(`/api/profile/${id}`, {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

