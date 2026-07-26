using System.Security.Claims;
using Application.Abstractions;

namespace WebAPI.Services
{
    public sealed class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

        public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

        public string? UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier);

        public string? Email => Principal?.FindFirstValue(ClaimTypes.Email)
                                ?? Principal?.FindFirstValue("email");

        public IReadOnlyCollection<string> Roles =>
            Principal?.FindAll(ClaimTypes.Role).Select(claim => claim.Value).ToArray() ?? [];

        public string? IpAddress =>
            _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

        public bool IsInRole(string role) => Principal?.IsInRole(role) ?? false;
    }
}
