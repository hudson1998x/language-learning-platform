export interface ConfigHelpInfo {
    component: string;
    tabName?: string | null;
}

export interface ConfigFieldInfo {
    type: string;
    value?: unknown | null;
    component?: string | null;
    help?: ConfigHelpInfo[] | null;
}

export interface ConfigTypeInfo {
    fields: Record<string, ConfigFieldInfo>;
    help?: ConfigHelpInfo[] | null;
}

export interface ApiResponse<T> {
    success: boolean;
    message?: string | null;
    data?: T | null;
}

export const listConfigs = (): Promise<ApiResponse<Record<string, ConfigTypeInfo>>> => {
    return fetch('/api/configuration/list', {
        method: "GET",
    })
    .then((response: Response) => {
        if (!response.ok) {
            throw new Error(`Request failed with status ${response.status}`);
        }
        return response.json();
    });
};

export enum JsonValueKind {
    Undefined = "Undefined",
    Object = "Object",
    Array = "Array",
    String = "String",
    Number = "Number",
    True = "True",
    False = "False",
    Null = "Null",
}

export interface JsonElement {
    valueKind: JsonValueKind;
}

export interface ConfigurationChangeRequest {
    configurationType: string;
    configuration: JsonElement;
}

export interface ConfigurationChangeResponse {
}

export const changeSettings = (payload: ConfigurationChangeRequest): Promise<ApiResponse<ConfigurationChangeResponse>> => {
    return fetch('/api/configuration/update', {
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

