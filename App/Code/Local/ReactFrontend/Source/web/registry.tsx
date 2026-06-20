import { FC } from 'react'

/**
 * Global, in-memory registry of React components, keyed by name.
 *
 * Components are registered via {@link register} and looked up via {@link mod}.
 * Stored directly on `window` so the registry survives across separate
 * script bundles/entry points that share the same page.
 */
window.__canvas_components = {};

/**
 * Registers a component under the given name, making it resolvable via {@link mod}.
 *
 * Re-registering an existing name overwrites the previous component.
 *
 * @param name - The unique key the component will be looked up by.
 * @param node - The React function component to register.
 */
export const register = (name: string, node: FC) => {
    window.__canvas_components[name] = node;
}

/**
 * Resolves a registered component by name.
 *
 * @param name - The key the component was registered under via {@link register}.
 * @returns The registered component, or a fallback component rendering an
 * "Unknown component" error message if no component is registered under `name`.
 */
export const mod = (name: string): FC => {

    if (window.__canvas_components[name])
    {
        return window.__canvas_components[name];
    }

    return () => <p className={'error'}>{`Unknown component ${name}`}</p>
}

export default mod

declare global
{
    interface Window
    {
        /**
         * Registry of React components, keyed by name.
         *
         * @see register
         * @see mod
         */
        __canvas_components: Record<string, FC>
    }
}