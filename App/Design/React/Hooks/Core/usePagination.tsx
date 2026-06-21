import { usePromise } from '@hook/usePromise'
import { ApiResponse } from '@api/user'
import { useState } from "react";

export type Deferred<T> = (page: string, size: string) => Promise<ApiResponse<T>>

export const usePagination = <T,>(callable: Deferred<T>, deps: unknown[]) => {

    const [page, setPage] = useState<number>(1)
    const [size, setSize] = useState<number>(20)

    const [results, isLoading, error] = usePromise(
        () => callable(String(page), String(size)),
        [...deps, page, size]
    );

    return {
        page,
        size,
        setSize,
        nextPage: () => setPage((prev) => prev + 1),
        prevPage: () => setPage((prev) => prev <= 1 ? 1 : prev - 1),
        results: results?.data ?? [],
        isLoading,
        error
    }
}