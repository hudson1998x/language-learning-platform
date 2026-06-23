import {
    FC,
    PropsWithChildren,
    createContext,
    useContext,
    useState,
    useCallback, useEffect,
} from "react";
import { Language, listAllLanguage, getCurrentLanguage, changeLanguage } from "@api/language";
import { usePromise } from "@hook/usePromise";

interface LanguageContextValue {
    language: Language | undefined;
    availableLanguages: Language[];
    isLoading: boolean;
    error: unknown;
    setLanguage: (lang: Language) => void;
}

const LanguageContext = createContext<LanguageContextValue | undefined>(
    undefined
);

export const LanguageProvider: FC<PropsWithChildren> = ({ children }) => {
    const [response, isLoading, error] = usePromise(listAllLanguage, []);
    const availableLanguages: Language[] = (response as any)?.data ?? [];

    const [selectedId, setSelectedId] = useState<string | null>(null);

    useEffect(() => {
        getCurrentLanguage().then((response) => {
            if (response?.data?.id) {
                setSelectedId(response.data.id);
            }
        });
    }, []);

    const language = availableLanguages.find((l) => l.id === selectedId);

    const setLanguage = useCallback((lang: Language) => {
        setSelectedId(lang.id);
        changeLanguage(lang.id);
    }, []);

    return (
        <LanguageContext.Provider
            value={{ language, availableLanguages, isLoading, error, setLanguage }}
        >
            {children}
        </LanguageContext.Provider>
    );
};

export const useLanguage = (): LanguageContextValue => {
    const ctx = useContext(LanguageContext);
    if (ctx === undefined) {
        throw new Error("useLanguage must be used within a LanguageProvider");
    }
    return ctx;
};
