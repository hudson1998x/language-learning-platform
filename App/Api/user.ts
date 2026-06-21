export interface User {
    email: string;
    password: string;
    fullName: string;
    roleId?: string | null;
    id: string;
    createTime: string;
    updateTime: string;
}

export interface ApiResponse<T> {
    success: boolean;
    message?: string | null;
    data?: T | null;
}

export const createUser = (payload: User): Promise<ApiResponse<User>> => {
    return fetch('/api/user/create', {
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

export const updateUser = (payload: User): Promise<ApiResponse<User>> => {
    return fetch('/api/user/update', {
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

export const deleteUser = (payload: User): Promise<ApiResponse<User>> => {
    return fetch('/api/user/delete', {
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

export const listAllUser = (): Promise<ApiResponse<User[]>> => {
    return fetch('/api/user/list', {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const listUserPaged = (): Promise<ApiResponse<User[]>> => {
    return fetch('/api/user/list/{pageNum}/{size}', {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const loadUser = (): Promise<ApiResponse<User>> => {
    return fetch('/api/user/{id}', {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

