import { useState, FormEvent } from 'react'
import './style.scss'
import { userLogin } from '@api/auth'

export const AppSignin = () => {
    const [email, setEmail] = useState('')
    const [password, setPassword] = useState('')
    const [showPassword, setShowPassword] = useState(false)
    const [isSubmitting, setIsSubmitting] = useState(false)
    const [error, setError] = useState('')

    const handleSubmit = (e: FormEvent) => {
        e.preventDefault()
        setError('')

        if (!email || !password) {
            setError('Enter your email and password to continue.')
            return
        }

        setIsSubmitting(true)
        // Replace with real auth call
        userLogin({ email, password }).then((response) => {
            if (response.success)
            {
                location.reload()
            }
            else {
                setError(response.message)
            }
            
            setIsSubmitting(false)
        })
        .catch(error => {
            setIsSubmitting(false)
            setError(error.message)
        })
    }

    return (
        <div className={'lle-signin-page'}>
            <div className={'lle-signin-page__card'}>
                <h1 className={'lle-signin-page__title'}>Sign in</h1>
                <p className={'lle-signin-page__subtitle'}>
                    Enter your details to continue
                </p>


                {error && (
                    <>
                        <div className={'lle-signin-page__form-error'} role={'alert'}>
                            {error}
                        </div>
                        <br />
                    </>
                )}

                <form className={'lle-signin-page__form'} onSubmit={handleSubmit} noValidate>
                    <div className={'lle-signin-page__form-field'}>
                        <label className={'lle-signin-page__form-label'} htmlFor={'email'}>
                            Email
                        </label>
                        <input
                            id={'email'}
                            name={'email'}
                            type={'email'}
                            className={'lle-signin-page__form-input'}
                            placeholder={'you@example.com'}
                            autoComplete={'email'}
                            value={email}
                            onChange={(e) => setEmail(e.target.value)}
                        />
                    </div>

                    <div className={'lle-signin-page__form-field'}>
                        <div className={'lle-signin-page__form-label-row'}>
                            <label className={'lle-signin-page__form-label'} htmlFor={'password'}>
                                Password
                            </label>
                            <a className={'lle-signin-page__form-link'} href={'#forgot-password'}>
                                Forgot password?
                            </a>
                        </div>
                        <div className={'lle-signin-page__form-input-wrap'}>
                            <input
                                id={'password'}
                                name={'password'}
                                type={showPassword ? 'text' : 'password'}
                                className={'lle-signin-page__form-input'}
                                placeholder={'Enter your password'}
                                autoComplete={'current-password'}
                                value={password}
                                onChange={(e) => setPassword(e.target.value)}
                            />
                            <button
                                type={'button'}
                                className={'lle-signin-page__form-toggle'}
                                onClick={() => setShowPassword((v) => !v)}
                                aria-label={showPassword ? 'Hide password' : 'Show password'}
                            >
                                {showPassword ? 'Hide' : 'Show'}
                            </button>
                        </div>
                    </div>

                    <button
                        type={'submit'}
                        className={'lle-signin-page__form-submit'}
                        disabled={isSubmitting}
                    >
                        {isSubmitting ? 'Signing in…' : 'Sign in'}
                    </button>
                </form>
            </div>
        </div>
    )
}