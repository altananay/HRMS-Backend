using Application.Abstractions;
using Application.Abstractions.Repositories;
using Microsoft.Extensions.Caching.Memory;

namespace Infrastructure.Services.Security
{
    public sealed class CachedUserSecurityStateProvider : IUserSecurityStateProvider
    {
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

            _cache.Set(CacheKey(userId), state, CacheLifetime);

            return state;
        }

        public void Invalidate(Guid userId) => _cache.Remove(CacheKey(userId));

        private static string CacheKey(Guid userId) => $"usersec:{userId}";
    }
}
