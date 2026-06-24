import { FC } from 'react';
import { LanguageSelector } from '@component/LanguageSelector';
import { useLanguage } from '@hook/language-provider';
import './style.scss';

interface LanguageWelcomeProps {
    onLanguageChange: (language: string) => void;
}

export const LanguageWelcome: FC<LanguageWelcomeProps> = ({ onLanguageChange }) => {
    const { setLanguage } = useLanguage();

    const handleLanguageChange = (lang: string) => {
        setLanguage(lang);
        onLanguageChange(lang);
    };

    return (
        <div className="lw-backdrop">
            <div className="lw-card">
                <div className="lw-icon">🌍</div>
                <h1 className="lw-title">Choose a language</h1>
                <p className="lw-subtitle">
                    Select the language you'd like to learn and we'll get you started.
                </p>
                <LanguageSelector onChange={handleLanguageChange} />
            </div>
        </div>
    );
};