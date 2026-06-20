import { FC, PropsWithChildren } from "react";

const DefaultTheme: FC<PropsWithChildren> = (props) => {
    
    return (
        <div className={'frontend-default-theme'}>
            {props.children}
        </div>
    )
}

export default DefaultTheme;