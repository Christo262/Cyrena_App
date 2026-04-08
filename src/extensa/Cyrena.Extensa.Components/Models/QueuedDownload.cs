namespace Cyrena.Extensa.Models
{
    public class QueuedDownload
    {
        public required PluginServer Server { get; set; }
        public required string PackageId { get; set; }

        public double Progress { get; set; }
        public bool IsUpdate { get; set; }
    }
}
