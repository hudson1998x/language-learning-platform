import {FC} from "react";

export type CanvasNode = {
    t: string,
    p?: Record<string, unknown>,
    c: CanvasNode[]
}

export type CanvasProps = {
    children: CanvasNode
}

export const Canvas: FC<CanvasProps> = () => {
    return (<p>Canvas loaded</p>)
}