namespace LLE.Auth.Dto
{
    /// <summary>
    /// Represents the data submitted when a new user registers for an account.
    /// </summary>
    public class RegistrationPayload
    {
        /// <summary>The email address for the new account.</summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>The display name for the new account.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>The desired password for the new account.</summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>A repeat of <see cref="Password"/>, used to confirm the user entered it correctly.</summary>
        public string PasswordRepeat { get; set; } = string.Empty;
    }
}