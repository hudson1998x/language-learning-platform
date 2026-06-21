import { useSession } from '@hook/session-provider'
import './style.scss'

const getInitials = (name?: string, email?: string): string => {
    if (name && name.trim().length > 0) {
        const parts = name.trim().split(/\s+/)
        const first = parts[0]?.[0] ?? ''
        const last = parts.length > 1 ? parts[parts.length - 1]?.[0] ?? '' : ''
        return (first + last).toUpperCase()
    }
    if (email) {
        return email[0]?.toUpperCase() ?? '?'
    }
    return '?'
}

export const UserCard: React.FC = () => {

    const { session } = useSession()
    const { user } = session

    const initials = getInitials(user?.fullName, user?.email)
    const displayName = user?.fullName && user.fullName.trim().length > 0
        ? user.fullName
        : user?.email

    return (
        <div onClick={() => location.pathname = '/account'} className={'user-card'}>
            <div className={'user-card__avatar'} aria-hidden={'true'}>
                {initials}
            </div>
            <div className={'user-card__meta'}>
                <span className={'user-card__name'}>{displayName}</span>
                {user?.fullName && (
                    <span className={'user-card__email'}>{user.email}</span>
                )}
            </div>
        </div>
    )
}