import { createRoot } from 'react-dom/client'
import {Canvas, CanvasNode} from "./canvas";
import './global.scss'

const root = createRoot(document.getElementById('app'));

root.render(
    <Canvas>
        {window.canvasState}   
    </Canvas>
)

declare global {
    interface Window {
        canvasState?: CanvasNode;
    }
}
