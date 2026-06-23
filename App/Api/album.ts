export interface Album {
    title: string;
    albumKey: string;
    albumCoverImageUrl: string;
    id: string;
    createTime: string;
    updateTime: string;
}

export interface ApiResponse<T> {
    success: boolean;
    message?: string | null;
    data?: T | null;
}

export const createAlbum = (payload: Album): Promise<ApiResponse<Album>> => {
    return fetch('/api/album/create', {
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

export const updateAlbum = (payload: Album): Promise<ApiResponse<Album>> => {
    return fetch('/api/album/update', {
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

export const deleteAlbum = (payload: Album): Promise<ApiResponse<Album>> => {
    return fetch('/api/album/delete', {
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

export const deleteAlbumById = (id: string): Promise<ApiResponse<Album>> => {
    return fetch(`/api/album/deleteById/${id}`, {
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

export const listAllAlbum = (): Promise<ApiResponse<Album[]>> => {
    return fetch('/api/album/list', {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const listAlbumPaged = (pageNum: string, size: string): Promise<ApiResponse<Album[]>> => {
    return fetch(`/api/album/list/${pageNum}/${size}`, {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const listAlbumPagedSorted = (pageNum: string, size: string, sortField: string, sortDir: string): Promise<ApiResponse<Album[]>> => {
    return fetch(`/api/album/list/${pageNum}/${size}/${sortField}/${sortDir}`, {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export const loadAlbum = (id: string): Promise<ApiResponse<Album>> => {
    return fetch(`/api/album/${id}`, {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

