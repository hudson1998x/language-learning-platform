import { UserCard } from '../UserCard'
import { NavBar } from '../Navbar'
import { LanguageSelector } from '@component/LanguageSelector'
import './style.scss'

export const Topbar: React.FC = () => {
    
    return (
        <div className={'lle-topbar'}>
            <div className={'logo-box'}>
                LLE
            </div>
            <LanguageSelector />
            <div className={'nav-container'}>
                <NavBar />
            </div>
            <UserCard />
        </div>
    )
}