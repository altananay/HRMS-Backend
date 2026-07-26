using System.ComponentModel.DataAnnotations;

namespace Infrastructure.Services.Storage
{
    public sealed class R2Options
    {
        public const string SectionName = "Storage:R2";

        [Required]
        public string AccountId { get; set; } = null!;

        [Required]
        public string AccessKeyId { get; set; } = null!;

        [Required]
        public string SecretAccessKey { get; set; } = null!;

        [Required]
        public string BucketName { get; set; } = null!;

        public string? ServiceUrl { get; set; }

        public string ResolveServiceUrl() => string.IsNullOrWhiteSpace(ServiceUrl)
            ? $"https://{AccountId}.r2.cloudflarestorage.com"
            : ServiceUrl;
    }
}
