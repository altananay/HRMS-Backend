namespace Infrastructure.Services.Storage
{
    internal static class StorageKey
    {
        public static string Build(string containerName, string fileNameOrKey)
        {
            var prefix = $"{containerName}/";

            return fileNameOrKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? fileNameOrKey
                : prefix + fileNameOrKey;
        }

        public static string StripContainer(string containerName, string fileNameOrKey)
        {
            var prefix = $"{containerName}/";

            return fileNameOrKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? fileNameOrKey[prefix.Length..]
                : fileNameOrKey;
        }
    }
}
