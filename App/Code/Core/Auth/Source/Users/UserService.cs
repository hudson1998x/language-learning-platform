using LLE.Auth.Dto;
using LLE.Auth.Exceptions;
using LLE.Auth.Utils;
using LLE.Kernel.Attributes;
using Microsoft.AspNetCore.Http;

namespace LLE.Auth.Users;

/// <summary>
/// Provides session-based authentication services, including user lookup,
/// login, and registration against the configured <see cref="IUserRepository"/>.
/// </summary>
[Service]
public class UserService(
    IUserRepository userRepository
)
{
    /// <summary>
    /// Resolves the currently authenticated user from the session, if any.
    /// </summary>
    /// <param name="httpContext">The current HTTP context whose session is used to look up the user.</param>
    /// <returns>
    /// The <see cref="User"/> associated with the session's <c>userId</c> value, or <c>null</c>
    /// if no user is logged in, the stored id is missing/invalid, or no matching user exists.
    /// </returns>
    /// <exception cref="InvalidOperationException">Thrown if the session is not available.</exception>
    public async Task<User?> GetCurrentUser(HttpContext httpContext)
    {
        EnsureSessionAvailable(httpContext);

        var userGuidStr = httpContext.Session.GetString("userId");
        
        if (string.IsNullOrEmpty(userGuidStr))
        {
            return null;
        }

        if (!Guid.TryParse(userGuidStr, out var userId))
        {
            return null;
        }
        
        return await userRepository.FindByIdAsync(userId);
    }
    
    /// <summary>
    /// Attempts to authenticate a user with the provided credentials and, on success,
    /// stores their id in the session.
    /// </summary>
    /// <param name="httpContext">The current HTTP context whose session will store the authenticated user's id.</param>
    /// <param name="loginData">The email and password submitted for login.</param>
    /// <exception cref="InvalidOperationException">Thrown if the session is not available.</exception>
    /// <exception cref="FailedAuthenticationException">
    /// Thrown when the email is empty (<see cref="FailCause.EmptyEmail"/>), the password is empty
    /// (<see cref="FailCause.EmptyPassword"/>), no user exists with the given email
    /// (<see cref="FailCause.InvalidEmail"/>), or the password does not match
    /// (<see cref="FailCause.IncorrectPassword"/>).
    /// </exception>
    public async Task TryLogin(HttpContext httpContext, LoginPayload loginData)
    {
        EnsureSessionAvailable(httpContext);
        
        if (string.IsNullOrEmpty(loginData.Email))
        {
            throw new FailedAuthenticationException(FailCause.EmptyEmail);
        }
        if (string.IsNullOrEmpty(loginData.Password))
        {
            throw new FailedAuthenticationException(FailCause.EmptyPassword);
        }

        var user = await userRepository.GetByEmailAsync(loginData.Email);

        if (user is null)
        {
            throw new FailedAuthenticationException(FailCause.InvalidEmail);
        }

        if (!PasswordHasher.Verify(loginData.Password, user.Password))
        {
            throw new FailedAuthenticationException(FailCause.IncorrectPassword);
        }
        
        httpContext.Session.SetString("userId", user.Id.ToString());
    }

    /// <summary>
    /// Validates registration data, creates a new user with a hashed password,
    /// and logs them in by storing their id in the session.
    /// </summary>
    /// <param name="httpContext">The current HTTP context whose session will store the new user's id.</param>
    /// <param name="registerData">The email, password, password confirmation, and name for the new account.</param>
    /// <exception cref="InvalidOperationException">Thrown if the session is not available.</exception>
    /// <exception cref="FailedRegistrationException">
    /// Thrown when the email is empty or invalid (<see cref="FailedRegistrationCause.EmailInvalid"/>),
    /// the password is empty (<see cref="FailedRegistrationCause.PasswordTooShort"/>), or the password
    /// and confirmation do not match (<see cref="FailedRegistrationCause.PasswordDoesNotMatch"/>).
    /// </exception>
    public async Task TryRegister(HttpContext httpContext, RegistrationPayload registerData)
    {
        EnsureSessionAvailable(httpContext);
        
        if (string.IsNullOrEmpty(registerData.Email))
        {
            throw new FailedRegistrationException(FailedRegistrationCause.EmailInvalid);
        }

        if (!EmailValid(registerData.Email, out var throwable))
        {
            throw throwable!;
        }

        if (string.IsNullOrEmpty(registerData.Password))
        {
            throw new FailedRegistrationException(FailedRegistrationCause.PasswordTooShort);
        }

        if (registerData.Password != registerData.PasswordRepeat)
        {
            throw new FailedRegistrationException(FailedRegistrationCause.PasswordDoesNotMatch);
        }
        
        var existing = await userRepository.GetByEmailAsync(registerData.Email);

        if (existing is not null)
        {
            throw new FailedRegistrationException(FailedRegistrationCause.EmailExists);
        }

        var user = new User
        {
            Email = registerData.Email,
            Password = PasswordHasher.Hash(registerData.Password),
            Name = registerData.Name
        };

        user = await userRepository.CreateAsync(user);
        
        httpContext.Session.SetString("userId", user.Id.ToString());
    }

    /// <summary>
    /// Performs a basic structural check on an email address (non-empty, contains
    /// an "@" and a ".").
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <param name="throwable">
    /// When this method returns <c>false</c>, contains a <see cref="FailedRegistrationException"/>
    /// describing why validation failed; otherwise <c>null</c>.
    /// </param>
    /// <returns><c>true</c> if the email passes the basic format check; otherwise <c>false</c>.</returns>
    private static bool EmailValid(string email, out FailedRegistrationException? throwable)
    {
        if (string.IsNullOrEmpty(email))
        {
            throwable = new FailedRegistrationException(FailedRegistrationCause.EmailInvalid);
            return false;
        }

        if (!email.Contains('@') || !email.Contains('.'))
        {
            throwable = new FailedRegistrationException(FailedRegistrationCause.EmailInvalid);
            return false;
        }

        throwable = null;
        return true;
    }

    /// <summary>
    /// Guards against operating on a session that has not been established.
    /// </summary>
    /// <param name="httpContext">The HTTP context whose session availability is checked.</param>
    /// <exception cref="InvalidOperationException">Thrown if <see cref="ISession.IsAvailable"/> is <c>false</c>.</exception>
    private static void EnsureSessionAvailable(HttpContext httpContext)
    {
        if (!httpContext.Session.IsAvailable)
        {
            throw new InvalidOperationException("The current session is not available.");
        }
    }
}