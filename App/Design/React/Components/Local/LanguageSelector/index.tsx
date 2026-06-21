import {FunctionComponent, useRef, useState, useEffect, useMemo} from "react";
import { useLanguage } from "@hook/language-provider";
import './style.scss';

export const LanguageSelector: FunctionComponent = () => {
    const { language, availableLanguages, setLanguage, isLoading } = useLanguage();
    const [open, setOpen] = useState(false);
    const [query, setQuery] = useState('');
    const ref = useRef<HTMLDivElement>(null);
    const searchRef = useRef<HTMLInputElement>(null);

    useEffect(() => {
        if (!open) return;

        const handleClick = (e: MouseEvent) => {
            if (ref.current && !ref.current.contains(e.target as Node)) {
                setOpen(false);
            }
        };
        const handleKey = (e: KeyboardEvent) => {
            if (e.key === 'Escape') setOpen(false);
        };

        document.addEventListener('mousedown', handleClick);
        document.addEventListener('keydown', handleKey);
        return () => {
            document.removeEventListener('mousedown', handleClick);
            document.removeEventListener('keydown', handleKey);
        };
    }, [open]);

    useEffect(() => {
        if (open) {
            searchRef.current?.focus();
        } else {
            setQuery('');
        }
    }, [open]);

    const filtered = useMemo(() => {
        const q = query.trim().toLowerCase();
        if (!q) return availableLanguages;
        return availableLanguages.filter((lang) =>
            lang.name.toLowerCase().includes(q)
        );
    }, [availableLanguages, query]);

    if (isLoading || availableLanguages.length === 0) {
        return (
            <div className={'language-selector'}>
                <div className={'language-selector-placeholder'}>Language</div>
            </div>
        );
    }

    return (
        <div className={'language-selector'} ref={ref}>
            <button
                className={'language-selector-current'}
                onClick={() => setOpen((prev) => !prev)}
                aria-expanded={open}
            >
                {language ? (
                    <>
                        <img
                            className={'flag-icon'}
                            src={`/media/languages/${language.flagIcon}.png`}
                            alt={''}
                        />
                        <span className={'current-text'}>
                            <span className={'current-eyebrow'}>Learning</span>
                            <span className={'current-name'}>{language.name}</span>
                        </span>
                    </>
                ) : (
                    <span className={'current-name'}>Choose a language</span>
                )}
                <span className={`chevron${open ? ' open' : ''}`}>
                    <svg width="10" height="6" viewBox="0 0 10 6" fill="none">
                        <path d="M1 1L5 5L9 1" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round"/>
                    </svg>
                </span>
            </button>

            {open && (
                <div className={'language-selector-dropdown'}>
                    <div className={'dropdown-header'}>
                        <span className={'dropdown-title'}>Pick a language to learn</span>
                        <div className={'search-field'}>
                            <svg className={'search-icon'} width="14" height="14" viewBox="0 0 14 14" fill="none">
                                <circle cx="6" cy="6" r="4.5" stroke="currentColor" strokeWidth="1.3"/>
                                <path d="M9.5 9.5L13 13" stroke="currentColor" strokeWidth="1.3" strokeLinecap="round"/>
                            </svg>
                            <input
                                ref={searchRef}
                                type="text"
                                value={query}
                                onChange={(e) => setQuery(e.target.value)}
                                placeholder="Search languages"
                            />
                        </div>
                    </div>

                    <div className={'language-grid'} role="listbox">
                        {filtered.length === 0 && (
                            <div className={'no-results'}>No languages match &ldquo;{query}&rdquo;</div>
                        )}
                        {filtered.map((lang) => {
                            const isActive = lang.id === language?.id;
                            return (
                                <button
                                    key={lang.id}
                                    role="option"
                                    aria-selected={isActive}
                                    className={`language-card${isActive ? ' active' : ''}`}
                                    onClick={() => {
                                        setLanguage(lang);
                                        setOpen(false);
                                    }}
                                >
                                    <img
                                        className={'flag-icon'}
                                        src={`/media/languages/${lang.flagIcon}.png`}
                                        alt={''}
                                    />
                                    <span className={'lang-name'}>{lang.name}</span>
                                    {isActive && (
                                        <span className={'active-badge'}>
                                            <svg width="11" height="9" viewBox="0 0 11 9" fill="none">
                                                <path d="M1 4.5L4 7.5L10 1" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round"/>
                                            </svg>
                                        </span>
                                    )}
                                </button>
                            );
                        })}
                    </div>
                </div>
            )}
        </div>
    );
};