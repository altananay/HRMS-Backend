namespace Application.Abstractions
{
    /// <summary>The two facts every authenticated request must re-check against the database.</summary>
    public sealed record UserSecurityState(Guid SecurityStamp, bool IsActive);

    /// <summary>
    /// Supplies the current security stamp and active flag for a user, so an access token can be
    /// invalidated before it expires.
    /// </summary>
    /// <remarks>
    /// A JWT is self-validating: once issued it stays valid until <c>exp</c> no matter what happens
    /// server-side. Without this check, five operations silently do nothing for up to 15 minutes —
    /// "log out everywhere", password change, account deactivation, role revocation, and (worst)
    /// refresh-token reuse detection, which concludes a token was stolen and then lets the thief keep
    /// making authenticated requests.
    ///
    /// The usual objection is that this reintroduces a database read per request and defeats the
    /// point of a stateless token. That argument applies to a distributed fleet of token verifiers;
    /// here there is one API and one database, and every authenticated request already queries
    /// PostgreSQL to do its actual work. One cached, primary-key lookup is noise by comparison.
    /// </remarks>
    public interface IUserSecurityStateProvider
    {
        Task<UserSecurityState?> GetAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>Drops the cached entry so a stamp change takes effect immediately.</summary>
        void Invalidate(Guid userId);
    }
}
