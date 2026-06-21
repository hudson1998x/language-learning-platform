import {FC, PropsWithChildren, useState} from "react";
import { SessionProvider, useSession } from '@hook/session-provider'
import { AppSignin } from './DefaultPages/SignIn'
import { AppRegister } from './DefaultPages/SignUp'
import './style.scss'
import { Topbar } from './Blocks/Topbar'

const LLEThemeWrapper: FC<PropsWithChildren> = (props) => {

    return (
        <SessionProvider>
            <LLETheme {...props}/>
        </SessionProvider>
    )
}

enum AuthPage
{
    SignIn,
    Register,
    Recovery
}

const LLETheme: FC<PropsWithChildren> = (props) => {

    const { session } = useSession();
    const [page, setPage] = useState<AuthPage>(AuthPage.SignIn)
    
    if (!session?.user)
    {
        // show a login UI.
        if (page == AuthPage.SignIn) {
            return (
                <AppSignin onSwitchToRegister={() => setPage(AuthPage.Register)} />
            )
        }

        if (page == AuthPage.Register) {
            return (
                <AppRegister onSwitchToSignIn={() => setPage(AuthPage.SignIn)} />
            )
        }
    }
    
    return (
        <div className={'lle-default-theme'}>
            <Topbar />
        </div>
    )
}

export default LLEThemeWrapper;