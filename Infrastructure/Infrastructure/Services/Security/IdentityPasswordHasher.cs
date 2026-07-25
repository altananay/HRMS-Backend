using Application.Abstractions;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services.Security
{
    /// <summary>
    /// PBKDF2-HMAC-SHA512 password hashing, delegating to ASP.NET Core Identity's implementation.
    /// </summary>
    /// <remarks>
    /// Named <c>IdentityPasswordHasher</c> rather than <c>PasswordHasher</c> to avoid colliding with
    /// the <see cref="PasswordHasher{TUser}"/> it wraps.
    ///
    /// Wrapping Identity's implementation rather than hand-rolling PBKDF2 is deliberate. Doing it by
    /// hand means owning salt generation, the encoding format, a version marker and a constant-time
    /// comparison — and hand-rolling exactly those four things is what produced the original
    /// <c>HashingHelper</c>: a single round of HMACSHA512 with no work factor, a verify loop that
    /// returned on the first mismatching byte, and an <c>IndexOutOfRangeException</c> waiting for any
    /// stored hash shorter than 64 bytes.
    ///
    /// It also reports <c>SuccessRehashNeeded</c>, which neither BCrypt.Net nor a hand-rolled version
    /// offers, so the work factor can be raised later and users upgrade silently on next sign-in.
    /// </remarks>
    public sealed class IdentityPasswordHasher : IPasswordHasher
    {
        // The generic argument is only a type token; PasswordHasher<T> never touches the instance,
        // which is why null is safe to pass below.
        private readonly PasswordHasher<User> _inner = new();

        public string Hash(string password) => _inner.HashPassword(null!, password);

        public PasswordVerificationOutcome Verify(string hashedPassword, string providedPassword)
        {
            // Guards against a malformed or empty stored hash rather than letting the framework throw
            // — the old helper indexed straight into the byte array and crashed.
            if (string.IsNullOrWhiteSpace(hashedPassword))
            {
                return PasswordVerificationOutcome.Failed;
            }

            try
            {
                return _inner.VerifyHashedPassword(null!, hashedPassword, providedPassword) switch
                {
                    PasswordVerificationResult.Success => PasswordVerificationOutcome.Success,
                    PasswordVerificationResult.SuccessRehashNeeded => PasswordVerificationOutcome.SuccessRehashNeeded,
                    _ => PasswordVerificationOutcome.Failed
                };
            }
            catch (FormatException)
            {
                // Stored value is not a valid hash payload; treat as a failed verification rather
                // than a 500.
                return PasswordVerificationOutcome.Failed;
            }
        }
    }
}
