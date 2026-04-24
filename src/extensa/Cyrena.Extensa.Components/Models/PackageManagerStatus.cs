namespace Cyrena.Extensa.Models
{
    public class PackageManagerStatus : EventArgs
    {
        public PackageManagerStatus() { }
        public PackageManagerStatus(bool isIndexing, int downloadCount, string? currentDownload, double downloadProgress)
        {
            IsIndexing = isIndexing;
            DownloadCount = downloadCount;
            CurrentDownload = currentDownload;
            DownloadProgress = downloadProgress;
        }
        public bool IsIndexing { get; }
        public int DownloadCount { get; }
        public string? CurrentDownload { get; }
        public double DownloadProgress { get; }
    }
}
