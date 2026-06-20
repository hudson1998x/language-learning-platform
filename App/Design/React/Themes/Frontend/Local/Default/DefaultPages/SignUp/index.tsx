import { useState, FormEvent } from 'react'
import { register, RegisterResponse } from '@api/auth'
import './style.scss'

export interface AppRegisterProps {
    onSwitchToSignIn?: () => void
}

export const AppRegister = ({ onSwitchToSignIn }: AppRegisterProps) => {
    const [email, setEmail] = useState('')
    const [password, setPassword] = useState('')
    const [confirmPassword, setConfirmPassword] = useState('')
    const [showPassword, setShowPassword] = useState(false)
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [error, setError] = useState('')
    const [success, setSuccess] = useState(false)

    const handleSubmit = (e: FormEvent) => {
        e.preventDefault()
        setError('')

        if (!email || !password || !confirmPassword) {
            setError('Fill in every field to create an account.')
            return
        }

        if (password !== confirmPassword) {
            setError('Passwords do not match.')
            return
        }

        setIsSubmitting(true)

        register({ email, password, confirmPassword })
            .then((response: RegisterResponse) => {
                if (!response.success) {
                    setError(response.message || 'Could not create your account.')
                    return
                }
                setSuccess(true)
            })
            .catch(() => {
                setError('Something went wrong. Check your connection and try again.')
            })
            .finally(() => {
                setIsSubmitting(false)
            })
    }

    if (success) {
        return (
            <div className={'lle-register-page'}>
                <div className={'lle-register-page__card'}>
                    <h1 className={'lle-register-page__title'}>Account created</h1>
                    <p className={'lle-register-page__subtitle'}>
                        You can now sign in with your new account.
                    </p>
                    <button
                        type={'button'}
                        className={'lle-register-page__form-submit'}
                        onClick={onSwitchToSignIn}
                    >
                        Go to sign in
                    </button>
                </div>
            </div>
        )
    }

    return (
        <div className={'lle-register-page'}>
            <div className={'lle-register-page__card'}>
                <h1 className={'lle-register-page__title'}>Create account</h1>
                <p className={'lle-register-page__subtitle'}>
                    Enter your details to get started
                </p>

                <form className={'lle-register-page__form'} onSubmit={handleSubmit} noValidate>
                    <div className={'lle-register-page__form-field'}>
                        <label className={'lle-register-page__form-label'} htmlFor={'email'}>
                            Email
                        </label>
                        <input
                            id={'email'}
                            name={'email'}
                            type={'email'}
                            className={'lle-register-page__form-input'}
                            placeholder={'you@example.com'}
                            autoComplete={'email'}
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                        />
                    </div>

                    <div className={'lle-register-page__form-field'}>
                        <label className={'lle-register-page__form-label'} htmlFor={'password'}>
                            Password
                        </label>
                        <div className={'lle-register-page__form-input-wrap'}>
                            <input
                                id={'password'}
                                name={'password'}
                                type={showPassword ? 'text' : 'password'}
                                className={'lle-register-page__form-input'}
                                placeholder={'Create a password'}
                                autoComplete={'new-password'}
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                            />
                            <button
                                type={'button'}
                                className={'lle-register-page__form-toggle'}
                                onClick={() => setShowPassword((v) => !v)}
                                aria-label={showPassword ? 'Hide password' : 'Show password'}
                            >
                                {showPassword ? 'Hide' : 'Show'}
                            </button>
                        </div>
                    </div>

                    <div className={'lle-register-page__form-field'}>
                        <label className={'lle-register-page__form-label'} htmlFor={'confirmPassword'}>
                            Confirm password
                        </label>
                        <input
                            id={'confirmPassword'}
                            name={'confirmPassword'}
                            type={showPassword ? 'text' : 'password'}
                            className={'lle-register-page__form-input'}
                            placeholder={'Re-enter your password'}
                            autoComplete={'new-password'}
                            value={confirmPassword}
                            onChange={(e) => setConfirmPassword(e.target.value)}
                        />
                    </div>

                    {error && (
                        <div className={'lle-register-page__form-error'} role={'alert'}>
                            {error}
                        </div>
                    )}

                    <button
                        type={'submit'}
                        className={'lle-register-page__form-submit'}
                        disabled={isSubmitting}
                    >
                        {isSubmitting ? 'Creating account…' : 'Create account'}
                    </button>
                </form>

                <p className={'lle-register-page__footer'}>
                    Already have an account?{' '}
                    <button
                        type={'button'}
                        className={'lle-register-page__footer-link'}
                        onClick={onSwitchToSignIn}
                    >
                        Sign in
                    </button>
                </p>
            </div>
        </div>
    )
}