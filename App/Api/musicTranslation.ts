export interface SongTranslationRequest {
    lyrics: string;
    title: string;
    artist: string;
    album: string;
}

export interface Track {
    title: string;
    albumId: string;
    artistId: string;
    songContents: string;
    id: string;
    createTime: string;
    updateTime: string;
}

export interface Artist {
    name: string;
    coverImageUrl: string;
    id: string;
    createTime: string;
    updateTime: string;
}

export interface Album {
    title: string;
    albumKey: string;
    albumCoverImageUrl: string;
    id: string;
    createTime: string;
    updateTime: string;
}

export interface SongTranslationResponse {
    song: Track;
    artist: Artist;
    album: Album;
}

export interface ApiResponse<T> {
    success: boolean;
    message?: string | null;
    data?: T | null;
}

export const translateSong = (payload: SongTranslationRequest): Promise<ApiResponse<SongTranslationResponse>> => {
    return fetch('/api/music/translateSong', {
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

