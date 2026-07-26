namespace Domain.Common
{
    public interface ISoftDeletable
    {
        DateTime? DeletedAt { get; set; }
    }
}
