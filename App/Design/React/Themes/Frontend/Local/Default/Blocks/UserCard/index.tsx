import { useSession } from '@hook/session-provider'

export const UserCard: React.FC = () => {
    
    const { session } = useSession();
    const { user } = session;
    
    return (
        <div className={'user-card'}>
            {user?.email}
        </div>
    )
}