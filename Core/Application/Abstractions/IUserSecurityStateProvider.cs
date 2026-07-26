namespace Application.Abstractions
{
    public sealed record UserSecurityState(Guid SecurityStamp, bool IsActive);

    public interface IUserSecurityStateProvider
    {
        Task<UserSecurityState?> GetAsync(Guid userId, CancellationToken cancellationToken = default);

        void Invalidate(Guid userId);
    }
}
