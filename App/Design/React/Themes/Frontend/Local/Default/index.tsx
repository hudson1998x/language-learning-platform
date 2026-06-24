import {FC, PropsWithChildren, useState} from "react";
import { SessionProvider, useSession } from '@hook/session-provider'
import { LanguageProvider, useLanguage } from '@hook/language-provider'
import { AppSignin } from './DefaultPages/SignIn'
import { AppRegister } from './DefaultPages/SignUp'
import './style.scss'
import { Topbar } from './Blocks/Topbar'
import { LanguageSelector } from '@component/LanguageSelector'
import { LanguageWelcome } from './DefaultPages/LanguageSelectionInterstitial'

const LLEThemeWrapper: FC<PropsWithChildren> = (props) => {

    return (
        <SessionProvider>
            <LanguageProvider>
                <LLETheme {...props}/>
            </LanguageProvider>
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
    const { language } = useLanguage();
    
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
    
    if (!language)
    {
        return (
            <div className={'lle-default-theme'}>
                <LanguageWelcome />
            </div>
        )
    }
    
    return (
        <div className={'lle-default-theme'}>
            <Topbar />
            <div className={'lle-default-page-content'}>
                {props.children}
            </div>
        </div>
    )
}

export default LLEThemeWrapper;