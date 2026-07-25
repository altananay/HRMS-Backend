using Application.Abstractions;
using Application.Abstractions.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services.Security
{
    /// <inheritdoc cref="IUserSecurityStateProvider"/>
    public sealed class CachedUserSecurityStateProvider : IUserSecurityStateProvider
    {
        /// <summary>
        /// Bounds how stale a security decision can be.
        /// </summary>
        /// <remarks>
        /// On a single instance revocation is genuinely immediate: whoever bumps the stamp also calls
        /// <see cref="Invalidate"/>, which evicts from this same process's cache. The TTL only
        /// matters if the API is scaled out — a second node could keep serving a revoked token for at
        /// most this long. If that ever becomes unacceptable, the options are a shorter TTL, no cache
        /// at all, or a distributed cache; the interface does not change either way.
        /// </remarks>
        private static readonly TimeSpan CacheLifetime = TimeSpan.FromSeconds(60);

        private readonly IUserRepository _users;
        private readonly IMemoryCache _cache;

        public CachedUserSecurityStateProvider(IUserRepository users, IMemoryCache cache)
        {
            _users = users;
            _cache = cache;
        }

        public async Task<UserSecurityState?> GetAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            if (_cache.TryGetValue(CacheKey(userId), out UserSecurityState? cached))
            {
                return cached;
            }

            var state = await _users.GetSecurityStateAsync(userId, cancellationToken);

            // A null result is cached too. Otherwise a token referencing a hard-deleted user would
            // hit the database on every single request.
            _cache.Set(CacheKey(userId), state, CacheLifetime);

            return state;
        }

        public void Invalidate(Guid userId) => _cache.Remove(CacheKey(userId));

        private static string CacheKey(Guid userId) => $"usersec:{userId}";
    }
}
