import { createRoot } from 'react-dom/client'
import {Canvas, CanvasNode} from "./canvas";
import './global.scss'
import { FrontendTheme, AdminTheme } from './generated.registry'

const root = createRoot(document.getElementById('app'));

root.render(
    <FrontendTheme>
        <Canvas>
            {window.canvasState}   
        </Canvas>
    </FrontendTheme>
)

declare global {
    interface Window {
        canvasState?: CanvasNode;
    }
}
