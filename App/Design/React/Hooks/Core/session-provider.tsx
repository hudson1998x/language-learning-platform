import {
    FC,
    PropsWithChildren,
    createContext,
    useContext,
} from "react";
import { userState } from "@api/auth";
import { usePromise } from "@hook/usePromise";

// Adjust this type to match whatever `userState` actually resolves to
type Session = Awaited<ReturnType<typeof userState>>;

interface SessionContextValue {
    session: Session | undefined;
    isLoading: boolean;
    error: unknown;
    isAdmin: boolean;
}

const SessionContext = createContext<SessionContextValue | undefined>(
    undefined
);

export const SessionProvider: FC<PropsWithChildren> = ({ children }) => {
    const [session, isLoading, error] = usePromise(userState, []);
    const isAdmin = session?.role?.key === 'admin';
    
    console.log({ isAdmin })

    return (
        <SessionContext.Provider value={{ session, isLoading, error, isAdmin }}>
            {children}
        </SessionContext.Provider>
    );
};

export const useSession = (): SessionContextValue => {
    const ctx = useContext(SessionContext);
    if (ctx === undefined) {
        throw new Error("useSession must be used within a SessionProvider");
    }
    return ctx;
};