import { useState, useEffect, useRef } from 'react';

export const usePromise = (promiseFactory, deps) => {
    const [result, setResult] = useState(null);
    const [isLoading, setIsLoading] = useState(true);
    const [error, setError] = useState(null);

    // Tracks the "current" call so stale responses from outdated
    // deps don't overwrite newer state.
    const callId = useRef(0);

    useEffect(() => {
        let cancelled = false;
        const id = ++callId.current;

        setIsLoading(true);
        setError(null);

        Promise.resolve()
            .then(() => promiseFactory())
            .then((value) => {
                if (cancelled || id !== callId.current) return;
                setResult(value);
                setIsLoading(false);
            })
            .catch((err) => {
                if (cancelled || id !== callId.current) return;
                setError(err);
                setIsLoading(false);
            });

        return () => {
            cancelled = true;
        };
        // eslint-disable-next-line react-hooks/exhaustive-deps
    }, deps);

    return [result, isLoading, error];
}
