export interface ApiResponse<T> {
    success: boolean;
    message?: string | null;
    data?: T | null;
}

export const listOllamaModels = (): Promise<ApiResponse<string[]>> => {
    return fetch('/api/ollama/models', {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

