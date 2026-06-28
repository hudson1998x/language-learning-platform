export interface LlmStatusResponse {
    available: boolean;
    defaultProvider?: string | null;
    providers: Record<string, boolean>;
    providerLogos: Record<string, string>;
    providerDescriptions: Record<string, string>;
}

export interface ApiResponse<T> {
    success: boolean;
    message?: string | null;
    data?: T | null;
}

export const llmStatus = (): Promise<ApiResponse<LlmStatusResponse>> => {
    return fetch('/api/llm/status', {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

