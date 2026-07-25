using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services.Storage
{
    /// <summary>
    /// Cloudflare R2 connection settings, validated at startup when R2 is the active provider.
    /// </summary>
    public sealed class R2Options
    {
        public const string SectionName = "Storage:R2";

        /// <summary>Cloudflare account id — forms the endpoint host.</summary>
        [Required]
        public string AccountId { get; set; } = null!;

        [Required]
        public string AccessKeyId { get; set; } = null!;

        [Required]
        public string SecretAccessKey { get; set; } = null!;

        [Required]
        public string BucketName { get; set; } = null!;

        /// <summary>
        /// S3 endpoint. Defaults to R2's, but is overridable so the same adapter can point at
        /// AWS S3, MinIO or DigitalOcean Spaces without a code change.
        /// </summary>
        public string? ServiceUrl { get; set; }

        public string ResolveServiceUrl() => string.IsNullOrWhiteSpace(ServiceUrl)
            ? $"https://{AccountId}.r2.cloudflarestorage.com"
            : ServiceUrl;
    }
}
