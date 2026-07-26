using Application.Abstractions;
using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Infrastructure.Services.Security
{
    public sealed class IdentityPasswordHasher : IPasswordHasher
    {
        private readonly PasswordHasher<User> _inner = new();

        public string Hash(string password) => _inner.HashPassword(null!, password);

        public PasswordVerificationOutcome Verify(string hashedPassword, string providedPassword)
        {
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
                return PasswordVerificationOutcome.Failed;
            }
        }
    }
}
