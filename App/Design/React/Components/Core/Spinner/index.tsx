import './style.scss';

/**
 * Spinner — a simple animated loading indicator.
 *
 * Props:
 *  - size: 'sm' | 'md' | 'lg' (default 'md')
 *  - color: any valid CSS color (default uses theme accent)
 *  - label: accessible label for screen readers (default 'Loading')
 */
export const Spinner = ({ size = 'md', color, label = 'Loading' })=> {
    const style = color ? { '--spinner-color': color } : undefined;

    return (
        <div
            className={`spinner spinner--${size}`}
            style={style}
            role="status"
            aria-label={label}
        >
            <span className="spinner__circle" />
            <span className="spinner__sr-only">{label}…</span>
        </div>
    );
}