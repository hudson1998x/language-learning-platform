import { UserCard } from '../UserCard'
import { NavBar } from '../Navbar'
import './style.scss'

export const Topbar: React.FC = () => {
    
    return (
        <div className={'lle-topbar'}>
            <div className={'logo-box'}>
                LLE
            </div>
            <div className={'nav-container'}>
                <NavBar />
            </div>
            <UserCard />
        </div>
    )
}