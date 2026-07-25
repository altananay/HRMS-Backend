namespace Infrastructure.Services.Storage
{
    /// <summary>
    /// Builds the provider key for a stored file.
    /// </summary>
    /// <remarks>
    /// Shared so the implementations cannot disagree about what <c>StoredFile.StoragePath</c> means.
    /// They did: upload returned a path that already carried the container prefix
    /// (<c>cv-files/abc.pdf</c>), and download passed both the container and that path back in.
    /// R2Storage happened to strip the duplicate, LocalStorage did not, so every local download
    /// looked for <c>cv-files/cv-files/abc.pdf</c> and 404'd — a file could be uploaded but never
    /// read back.
    ///
    /// The contract is now explicit: <c>StoragePath</c> is the full key including the container, and
    /// re-prefixing an already-prefixed key is a no-op.
    /// </remarks>
    internal static class StorageKey
    {
        public static string Build(string containerName, string fileNameOrKey)
        {
            var prefix = $"{containerName}/";

            return fileNameOrKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? fileNameOrKey
                : prefix + fileNameOrKey;
        }

        /// <summary>The key with its container prefix removed, for filesystem path composition.</summary>
        public static string StripContainer(string containerName, string fileNameOrKey)
        {
            var prefix = $"{containerName}/";

            return fileNameOrKey.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)
                ? fileNameOrKey[prefix.Length..]
                : fileNameOrKey;
        }
    }
}
