import {FC, PropsWithChildren} from "react";
import { SessionProvider, useSession } from '@hook/session-provider'
import { AppSignin } from './DefaultPages/SignIn'

const LLEThemeWrapper: FC<PropsWithChildren> = (props) => {
    
    return (
        <SessionProvider>
            <LLETheme {...props}/>
        </SessionProvider>
    )
}

const LLETheme: FC<PropsWithChildren> = (props) => {
    
    const { user, role } = useSession();
    
    if (!user)
    {
        // show a login UI.
        return (
            <AppSignin />
        )
    }
    
    return (
        <div className={'lle-default-theme'}>
            
        </div>
    )
}

export default LLEThemeWrapper;