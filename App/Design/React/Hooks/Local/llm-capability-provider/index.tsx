import { FC, PropsWithChildren, createContext, useContext } from "react";
import { llmStatus } from "@api/llm";
import { usePromise } from "@hook/usePromise";

type LlmStatus = Awaited<ReturnType<typeof llmStatus>>;

interface LlmCapabilityContextValue {
    status: LlmStatus | undefined;
    isLoading: boolean;
    error: unknown;
    available: boolean;
    hasOllama: boolean;
}

const LlmCapabilityContext = createContext<LlmCapabilityContextValue | undefined>(
    undefined
);

export const LlmCapabilityProvider: FC<PropsWithChildren> = ({ children }) => {
    const [status, isLoading, error] = usePromise(llmStatus, []);
    const available = status?.data?.available ?? false;
    const hasOllama = status?.data?.providers?.Ollama ?? false;

    return (
        <LlmCapabilityContext.Provider value={{ status, isLoading, error, available, hasOllama }}>
            {children}
        </LlmCapabilityContext.Provider>
    );
};

export const useLlmCapability = (): LlmCapabilityContextValue => {
    const ctx = useContext(LlmCapabilityContext);
    if (ctx === undefined) {
        throw new Error("useLlmCapability must be used within a LlmCapabilityProvider");
    }
    return ctx;
};
