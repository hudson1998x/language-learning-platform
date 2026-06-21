import { usePromise } from '@hook/usePromise'
import { ApiResponse } from '@api/user'
import { useState } from "react";

export type Deferred<T> = (page: string, size: string, sortField?: string, sortDir?: string) => Promise<ApiResponse<T>>

export const usePagination = <T,>(callable: Deferred<T>, deps: unknown[]) => {

    const [page, setPage] = useState<number>(1)
    const [size, setSize] = useState<number>(20)
    const [sortField, setSortField] = useState<string | undefined>('createTime')
    const [sortDir, setSortDir] = useState<string | undefined>('desc')

    const [results, isLoading, error] = usePromise(
        () => callable(String(page), String(size), sortField, sortDir),
        [...deps, page, size, sortField, sortDir]
    );

    return {
        page,
        size,
        setSize,
        sortField,
        sortDir,
        setSortField,
        setSortDir,
        nextPage: () => setPage((prev) => prev + 1),
        prevPage: () => setPage((prev) => prev <= 1 ? 1 : prev - 1),
        results: results?.data ?? [],
        isLoading,
        error
    }
}