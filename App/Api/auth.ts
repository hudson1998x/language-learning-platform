export interface LoginBody {
    email: string;
    password: string;
}

export interface User {
    email: string;
    password: string;
    roleId?: string | null;
    id: string;
    createTime: string;
    updateTime: string;
}

export interface LoginResponse {
    user?: User | null;
    success: boolean;
    message: string;
}

export const userLogin = (payload: LoginBody): Promise<LoginResponse> => {
    return fetch('/api/auth/login', {
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

export interface Role {
    key: string;
    name: string;
    description: string;
    id: string;
    createTime: string;
    updateTime: string;
}

export interface UserAuthStateResponse {
    user?: User | null;
    role?: Role | null;
    success: boolean;
    message: string;
}

export const userState = (): Promise<UserAuthStateResponse> => {
    return fetch('/api/auth/state', {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export interface RegisterBody {
    email: string;
    password: string;
    confirmPassword: string;
}

export interface RegisterResponse {
    user: User;
    success: boolean;
    message: string;
}

export const register = (payload: RegisterBody): Promise<RegisterResponse> => {
    return fetch('/api/auth/register', {
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

