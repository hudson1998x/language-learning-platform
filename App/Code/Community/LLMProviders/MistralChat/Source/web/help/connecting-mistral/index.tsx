import { register } from "@registry";
import './style.scss';
import { useState } from 'react';

const steps = [
    {
        number: '01',
        heading: 'Create or sign in to your Mistral account',
        body: 'Head over to the Mistral console. You\'ll need an account — if you don\'t have one yet, registration is quick and free.',
        link: { href: 'https://console.mistral.ai/home', label: 'console.mistral.ai/home' },
        image: '/media/mistral/help/console.png',
        imageAlt: 'Mistral AI console home screen',
    },
    {
        number: '02',
        heading: 'Open the API Keys section',
        body: 'Once logged in, click "Create an API key" near the centre of the page. A dialog will appear — this is where you\'ll configure your new key.',
        image: '/media/mistral/help/my-api-keys.png',
        imageAlt: 'Mistral API keys management page',
    },
    {
        number: '03',
        heading: 'Name and configure your key',
        body: 'Enter a memorable name for your key. Under connector access approval, select both Private and Shared Connectors to ensure the key has the right permissions.',
        image: '/media/mistral/help/creating_api_key.png',
        imageAlt: 'Creating a new Mistral API key dialog',
    },
    {
        number: '04',
        heading: 'Copy your key — you\'ll only see it once',
        body: 'After clicking "Create new", Mistral will display your key. Copy it immediately and store it somewhere safe — it won\'t be shown again.',
        image: '/media/mistral/help/copy_key.png',
        imageAlt: 'Generated Mistral API key displayed on screen',
    },
    {
        number: '05',
        heading: 'Paste the key into your settings',
        body: 'Back in the connector settings, find the "ApiKey" field and paste in the key you just copied. Save, and you\'re connected.',
        image: '/media/mistral/help/input-in-apikey.png',
        imageAlt: 'Pasting the API key into the connector settings',
    },
];

// Thumbnail only — no overlay rendered here
const StepImage = ({ src, alt, onOpen }) => (
    <div className="step-image-wrap" onClick={() => onOpen({ src, alt })} title="Click to enlarge">
        <img src={src} alt={alt} className="step-image" />
        <span className="zoom-hint">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                <circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/>
                <line x1="11" y1="8" x2="11" y2="14"/><line x1="8" y1="11" x2="14" y2="11"/>
            </svg>
            Enlarge
        </span>
    </div>
);

// Overlay lives here — direct child of the root element, outside every step-card stacking context
const ZoomOverlay = ({ image, onClose }) => {
    if (!image) return null;
    return (
        <div className="zoom-overlay" onClick={onClose}>
            <div className="zoom-overlay__inner" onClick={e => e.stopPropagation()}>
                <button className="zoom-close" onClick={onClose} aria-label="Close">
                    <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
                        <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
                    </svg>
                </button>
                <img src={image.src} alt={image.alt} className="zoom-image" />
            </div>
        </div>
    );
};

export const ConnectingMistralHelp = () => {
    const [zoomedImage, setZoomedImage] = useState(null);

    return (
        <div className="mistral-help-connection-guide">
            <div className="guide-header">
                <span className="guide-eyebrow">Integration Guide</span>
                <h2 className="guide-title">Connecting to Mistral AI</h2>
                <p className="guide-subtitle">
                    Follow these five steps to generate an API key and link your Mistral account.
                    The whole process takes under two minutes.
                </p>
            </div>

            <ol className="steps-list">
                {steps.map(step => (
                    <li key={step.number} className="step-card">
                        <span className="step-number" aria-hidden="true">{step.number}</span>
                        <div className="step-content">
                            <h3 className="step-heading">{step.heading}</h3>
                            <p className="step-body">
                                {step.body}
                                {step.link && (
                                    <> Visit <a href={step.link.href} target="_blank" rel="noopener noreferrer">{step.link.label}</a>.</>
                                )}
                            </p>
                            <StepImage src={step.image} alt={step.imageAlt} onOpen={setZoomedImage} />
                        </div>
                    </li>
                ))}
            </ol>

            <div className="guide-footer">
                <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
                    <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
                </svg>
                Keep your API key private — never commit it to source control or share it publicly.
            </div>

            {/* Rendered here — sibling to all step-cards, no stacking context ancestors */}
            <ZoomOverlay image={zoomedImage} onClose={() => setZoomedImage(null)} />
        </div>
    );
};

register('@help/mistral/connection-guide', ConnectingMistralHelp);