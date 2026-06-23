import { useEffect, useState } from "react";
import { loadTrack, Track } from '@api/track'
import { loadArtist, Artist } from '@api/artist'
import { loadAlbum, Album } from '@api/album'
import { Spinner } from '@component/Spinner'
import { useSession } from '@hook/session-provider'
import { useLanguage } from '@hook/language-provider'
import { CreateFlashCardModal } from '@component/Pages/FlashCards/CreateFlashCardModal'

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
    const { session } = useSession()
    const { language } = useLanguage()
    const [track, setTrack] = useState<Track | null>(null)
    const [artist, setArtist] = useState<Artist | null>(null)
    const [album, setAlbum] = useState<Album | null>(null)
    const [lines, setLines] = useState<SongLine[]>([])
    const [expandedIndex, setExpandedIndex] = useState<number | null>(null)
    const [isLoading, setIsLoading] = useState(false)
    const [error, setError] = useState<string | null>(null)
    const [openMenuIndex, setOpenMenuIndex] = useState<number | null>(null)
    const [flashcardLineIndex, setFlashcardLineIndex] = useState<number | null>(null)

    useEffect(() => {
        if (!trackId) return

        setIsLoading(true)
        setError(null)
        setExpandedIndex(null)
        setOpenMenuIndex(null)
        setFlashcardLineIndex(null)

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
        setOpenMenuIndex(null)
    }

    const flashcardLine = flashcardLineIndex !== null ? lines[flashcardLineIndex] : null

    return (
        <div className={'detail-overlay'} onClick={onClose}>
            <div className={'detail-dialog'} onClick={(e) => {
                e.stopPropagation()
                setOpenMenuIndex(null)
            }}>
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
                            >
                                <div className={'line-header'}>
                                    <div
                                        className={'line-original'}
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
                                        {line.lineContents}
                                    </div>
                                    <div className={'line-actions'}>
                                        <button
                                            className={`line-action-btn${openMenuIndex === i ? ' active' : ''}`}
                                            onClick={(e) => {
                                                e.stopPropagation()
                                                setOpenMenuIndex((prev) => prev === i ? null : i)
                                            }}
                                            aria-label={'Actions'}
                                        >
                                            <svg width="14" height="14" viewBox="0 0 14 14" fill="none">
                                                <path d="M7 3V3.01M7 7V7.01M7 11V11.01" stroke="currentColor" strokeWidth="2" strokeLinecap="round"/>
                                            </svg>
                                        </button>
                                        {openMenuIndex === i && (
                                            <div className={'line-dropdown'}>
                                                <button
                                                    className={'line-dropdown-item'}
                                                    onClick={(e) => {
                                                        e.stopPropagation()
                                                        setOpenMenuIndex(null)
                                                        setFlashcardLineIndex(i)
                                                    }}
                                                >
                                                    <svg width="14" height="14" viewBox="0 0 14 14" fill="none">
                                                        <rect x="1" y="3" width="12" height="10" rx="1.5" stroke="currentColor" strokeWidth="1.3"/>
                                                        <path d="M4 1V3M10 1V3M1 6H13" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round"/>
                                                    </svg>
                                                    Create Flash Card
                                                </button>
                                            </div>
                                        )}
                                    </div>
                                </div>
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

                {flashcardLine && (
                    <CreateFlashCardModal
                        userId={session?.user?.id ?? ''}
                        languageId={language?.id ?? ''}
                        showLanguageSelector={true}
                        initialValues={{
                            frontStatement: flashcardLine.lineContents,
                            backStatement: flashcardLine.translationToUserLanguage,
                            pronunciation: flashcardLine.pronunciations.join(', '),
                            notes: `From ${track?.title ?? 'Unknown'} by ${artist?.name ?? 'Unknown'}`,
                            category: 'Music',
                            tags: 'music',
                        }}
                        onClose={() => setFlashcardLineIndex(null)}
                        onCreated={() => setFlashcardLineIndex(null)}
                    />
                )}
            </div>
        </div>
    )
}
