namespace Application.Abstractions
{
    public interface ICurrentUserService
    {
        string? UserId { get; }

        string? Email { get; }

        IReadOnlyCollection<string> Roles { get; }

        bool IsAuthenticated { get; }

        string? IpAddress { get; }

        bool IsInRole(string role);
    }
}
