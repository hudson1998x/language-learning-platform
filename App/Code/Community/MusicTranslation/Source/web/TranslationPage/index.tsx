import { useState, useMemo } from "react";
import { usePagination } from '@hook/usePagination'
import { usePromise } from '@hook/usePromise'
import {
    listTrackPagedSorted,
    Track
} from '@api/track'
import { listAllArtist, Artist } from '@api/artist'
import { listAllAlbum, Album } from '@api/album'
import { Spinner } from '@component/Spinner'
import { CreateSongModal } from './CreateSongModal'
import { SongDetailDialog } from './SongDetailDialog'
import { register } from '@registry'
import './style.scss'

const SORT_OPTIONS = [
    { value: 'createTime', label: 'Date added' },
    { value: 'title', label: 'Title' },
] as const

export const MusicTranslationIndexPage = () => {
    const [isCreateOpen, setIsCreateOpen] = useState(false)
    const [detailTrackId, setDetailTrackId] = useState<string | null>(null)
    const [refreshKey, setRefreshKey] = useState(0)

    const [selectedArtist, setSelectedArtist] = useState('')
    const [selectedAlbum, setSelectedAlbum] = useState('')
    const [searchQuery, setSearchQuery] = useState('')

    const [allArtists] = usePromise(() => listAllArtist(), [])
    const [allAlbums] = usePromise(() => listAllAlbum(), [])

    const artistMap = useMemo(() => {
        const map = new Map<string, string>()
        const data = (allArtists as { data?: Artist[] } | null)?.data ?? []
        for (const a of data) map.set(a.id, a.name)
        return map
    }, [allArtists])

    const albumMap = useMemo(() => {
        const map = new Map<string, string>()
        const data = (allAlbums as { data?: Album[] } | null)?.data ?? []
        for (const a of data) map.set(a.id, a.title)
        return map
    }, [allAlbums])

    const artistNames = useMemo(() => {
        const names = new Set(artistMap.values())
        return Array.from(names).sort()
    }, [artistMap])

    const albumNames = useMemo(() => {
        const names = new Set(albumMap.values())
        return Array.from(names).sort()
    }, [albumMap])

    const {
        page,
        size,
        nextPage,
        prevPage,
        results: tracks,
        isLoading: tracksLoading,
        error: tracksError,
        sortField,
        sortDir,
        setSortField,
        setSortDir,
    } = usePagination<Track[]>(listTrackPagedSorted, [refreshKey])

    const filteredTracks = useMemo(() => {
        let list = (tracks as Track[]) ?? []
        if (selectedArtist) {
            list = list.filter((t) => artistMap.get(t.artistId) === selectedArtist)
        }
        if (selectedAlbum) {
            list = list.filter((t) => albumMap.get(t.albumId) === selectedAlbum)
        }
        if (searchQuery.trim()) {
            const q = searchQuery.trim().toLowerCase()
            list = list.filter((t) => t.title.toLowerCase().includes(q))
        }
        return list
    }, [tracks, selectedArtist, selectedAlbum, searchQuery, artistMap, albumMap])

    const mayHaveNextPage = (tracks as Track[] | null)?.length === size

    const handleCreated = (trackId: string) => {
        setIsCreateOpen(false)
        setRefreshKey((k) => k + 1)
        setDetailTrackId(trackId)
    }

    if (tracksLoading) {
        return (
            <div className={'music-translation-status'}><Spinner /></div>
        )
    }

    if (tracksError) {
        return (
            <div className={'music-translation-status'}>
                <div className={'error'}>{`Couldn't load songs: ${tracksError}`}</div>
            </div>
        )
    }

    const trackList = filteredTracks

    return (
        <div className={'music-translation'}>
            <div className={'library-actions'}>
                <div className={'pagination'}>
                    <button onClick={prevPage} disabled={page <= 1}>Previous</button>
                    <span>{`Page ${page}`}</span>
                    <button onClick={nextPage} disabled={!mayHaveNextPage}>Next</button>
                </div>

                <div className={'sort-controls'}>
                    <select value={sortField ?? ''} onChange={(e) => setSortField(e.target.value || undefined)}>
                        {SORT_OPTIONS.map((opt) => (
                            <option key={opt.value} value={opt.value}>{opt.label}</option>
                        ))}
                    </select>
                    <select value={sortDir ?? 'desc'} onChange={(e) => setSortDir(e.target.value)}>
                        <option value="asc">Asc</option>
                        <option value="desc">Desc</option>
                    </select>
                </div>

                <div className={'filter-controls'}>
                    <select value={selectedArtist} onChange={(e) => setSelectedArtist(e.target.value)}>
                        <option value={''}>All artists</option>
                        {artistNames.map((name) => (
                            <option key={name} value={name}>{name}</option>
                        ))}
                    </select>
                    <select value={selectedAlbum} onChange={(e) => setSelectedAlbum(e.target.value)}>
                        <option value={''}>All albums</option>
                        {albumNames.map((name) => (
                            <option key={name} value={name}>{name}</option>
                        ))}
                    </select>
                    <input
                        type={'text'}
                        placeholder={'Search tracks...'}
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                    />
                </div>

                <button
                    className={'create-button'}
                    onClick={() => setIsCreateOpen(true)}
                >
                    Add song
                </button>
            </div>

            {trackList.length === 0 && (
                <div className={'notice'}>
                    <div className={'notice-title'}>No songs yet</div>
                    <div className={'notice-body'}>Click "Add song" to translate your first track.</div>
                </div>
            )}

            {trackList.length > 0 && (
                <table className={'song-table'}>
                    <thead>
                        <tr>
                            <th>Title</th>
                            <th>Artist</th>
                            <th>Album</th>
                        </tr>
                    </thead>
                    <tbody>
                        {trackList.map((track) => (
                            <tr
                                key={track.id}
                                className={'song-row'}
                                onClick={() => setDetailTrackId(track.id)}
                                role={'button'}
                                tabIndex={0}
                                onKeyDown={(e) => {
                                    if (e.key === 'Enter' || e.key === ' ') {
                                        e.preventDefault()
                                        setDetailTrackId(track.id)
                                    }
                                }}
                            >
                                <td className={'song-title'}>{track.title}</td>
                                <td className={'song-artist'}>{artistMap.get(track.artistId) ?? '...'}</td>
                                <td className={'song-album'}>{albumMap.get(track.albumId) ?? '...'}</td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            )}

            {isCreateOpen && (
                <CreateSongModal
                    onClose={() => setIsCreateOpen(false)}
                    onCreated={handleCreated}
                />
            )}

            {detailTrackId && (
                <SongDetailDialog
                    trackId={detailTrackId}
                    onClose={() => setDetailTrackId(null)}
                />
            )}
        </div>
    )
}

register('@page/music-translation-index', MusicTranslationIndexPage);
