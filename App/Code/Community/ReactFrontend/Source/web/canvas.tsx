import { createElement, FC } from "react";
import { mod } from './registry'

export type CanvasNode = {
    t: string,
    p?: Record<string, unknown>,
    c: CanvasNode[]
}

export type CanvasProps = {
    children: CanvasNode
}

/**
 * Renders a single {@link CanvasNode}, recursing into its children.
 *
 * Tags that are entirely lowercase are treated as plain HTML elements
 * (e.g. `div`, `p`, `span`). Any other tag is resolved to a registered
 * component via {@link mod}.
 */
const renderNode = (node: CanvasNode, key?: React.Key): React.ReactNode => {

    const { t, p, c } = node;

    const isHtmlElement = t === t.toLowerCase() && !t.startsWith('@');

    const children = c.map((child, index) => renderNode(child, index));

    if (isHtmlElement)
    {
        // JSX can't take a lowercase variable as a dynamic tag name (it would
        // compile to the literal string "t"), so createElement is used here instead.
        return createElement(t, { ...p, key }, ...children);
    }

    const Component: FC<unknown> = mod(t);
    
    // @ts-ignore 
    // this is because of InstrinsicAttributes complaining
    // about a mismatch in props when key and children
    // are there, it's fine though.
    return <Component {...p} key={key}>{children}</Component>;
}

export const Canvas: FC<CanvasProps> = (props: CanvasProps) => {

    const { children } = props;

    return <>{renderNode(children)}</>;
}

export default Canvas