namespace Cyrena.Extensa.Models
{
    /// <summary>
    /// Result of a package download operation.
    /// </summary>
    public class PackageDownloadResult : IDisposable
    {
        public required MemoryStream Stream { get; init; }
        public required string FileName { get; init; }
        public long ContentLength { get; init; }
        public string? ContentHash { get; init; }
        public string? Version { get; init; }

        public void Dispose()
        {
            Stream.Dispose();
        }
    }
}
