namespace Domain.Enums
{
    /// <summary>
    /// Where a stored file physically lives. Persisted as text on <c>cv_files</c>.
    /// </summary>
    /// <remarks>
    /// <c>Azure</c> is gone: Cloudflare R2 was chosen instead, and carrying an unselected provider's
    /// adapter is dead weight. R2 speaks the S3 API, so the same implementation also works against
    /// AWS S3, MinIO or DigitalOcean Spaces by changing the service URL.
    /// </remarks>
    public enum StorageProvider
    {
        Local = 0,
        R2 = 1
    }
}
