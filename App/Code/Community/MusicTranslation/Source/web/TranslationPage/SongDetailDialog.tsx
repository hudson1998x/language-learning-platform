import { useEffect, useState } from "react";
import { loadTrack, Track } from '@api/track'
import { loadArtist, Artist } from '@api/artist'
import { loadAlbum, Album } from '@api/album'
import { Spinner } from '@component/Spinner'

interface SongLine {
    lineContents: string;
    translationToUserLanguage: string;
    pronunciations: string[];
    culturalMeaning: string;
}

interface SongDetailDialogProps {
    trackId: string | null;
    onClose: () => void;
}

export const SongDetailDialog = ({ trackId, onClose }: SongDetailDialogProps) => {
    const [track, setTrack] = useState<Track | null>(null)
    const [artist, setArtist] = useState<Artist | null>(null)
    const [album, setAlbum] = useState<Album | null>(null)
    const [lines, setLines] = useState<SongLine[]>([])
    const [expandedIndex, setExpandedIndex] = useState<number | null>(null)
    const [isLoading, setIsLoading] = useState(false)
    const [error, setError] = useState<string | null>(null)

    useEffect(() => {
        if (!trackId) return

        setIsLoading(true)
        setError(null)
        setExpandedIndex(null)

        loadTrack(trackId)
            .then((res) => {
                if (!res.success || !res.data) {
                    throw new Error(res.message ?? 'Failed to load track')
                }
                const t = res.data
                setTrack(t)

                try {
                    const parsed: SongLine[] = JSON.parse(t.songContents)
                    setLines(Array.isArray(parsed) ? parsed : [])
                } catch {
                    setLines([])
                }

                return Promise.all([
                    loadArtist(t.artistId),
                    loadAlbum(t.albumId)
                ])
            })
            .then(([artistRes, albumRes]) => {
                if (artistRes.success && artistRes.data) setArtist(artistRes.data)
                if (albumRes.success && albumRes.data) setAlbum(albumRes.data)
                setIsLoading(false)
            })
            .catch((err) => {
                setError(err instanceof Error ? err.message : 'Failed to load song details')
                setIsLoading(false)
            })
    }, [trackId])

    if (!trackId) return null

    const toggleLine = (index: number) => {
        setExpandedIndex((prev) => prev === index ? null : index)
    }

    return (
        <div className={'detail-overlay'} onClick={onClose}>
            <div className={'detail-dialog'} onClick={(e) => e.stopPropagation()}>
                <div className={'detail-header'}>
                    <div className={'detail-meta'}>
                        <h2>{track?.title ?? 'Loading...'}</h2>
                        <div className={'detail-artist'}>{artist?.name ?? '...'}</div>
                        <div className={'detail-album'}>{album?.title ?? '...'}</div>
                    </div>
                    <button className={'detail-close'} onClick={onClose}>✕</button>
                </div>

                {isLoading && (
                    <div className={'detail-loading'}><Spinner /></div>
                )}

                {error && (
                    <div className={'detail-loading'}>
                        <div className={'error'}>{error}</div>
                    </div>
                )}

                {!isLoading && !error && lines.length === 0 && (
                    <div className={'detail-loading'}>
                        <div className={'notice-body'}>No lyrics available</div>
                    </div>
                )}

                {!isLoading && !error && lines.length > 0 && (
                    <div className={'detail-lyrics'}>
                        {lines.map((line, i) => (
                            <div
                                key={i}
                                className={`lyric-line ${expandedIndex === i ? 'expanded' : ''}`}
                                onClick={() => toggleLine(i)}
                                role={'button'}
                                tabIndex={0}
                                onKeyDown={(e) => {
                                    if (e.key === 'Enter' || e.key === ' ') {
                                        e.preventDefault()
                                        toggleLine(i)
                                    }
                                }}
                            >
                                <div className={'line-original'}>{line.lineContents}</div>
                                {expandedIndex === i && (
                                    <div className={'line-details'}>
                                        <div className={'detail-row translation'}>
                                            <span className={'detail-label'}>Translation:</span>
                                            <span className={'detail-value'}>{line.translationToUserLanguage}</span>
                                        </div>
                                        {line.pronunciations.length > 0 && (
                                            <div className={'detail-row'}>
                                                <span className={'detail-label'}>Pronunciation:</span>
                                                <span className={'detail-value'}>{line.pronunciations.join(', ')}</span>
                                            </div>
                                        )}
                                        {line.culturalMeaning && (
                                            <div className={'detail-row'}>
                                                <span className={'detail-label'}>Cultural meaning:</span>
                                                <span className={'detail-value'}>{line.culturalMeaning}</span>
                                            </div>
                                        )}
                                    </div>
                                )}
                            </div>
                        ))}
                    </div>
                )}
            </div>
        </div>
    )
}
