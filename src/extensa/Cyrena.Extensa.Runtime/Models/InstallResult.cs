namespace Cyrena.Extensa.Installer.Models
{
    public class InstallResult
    {
        public InstallResult()
        {
            Errors = new List<Exception>();
        }
        public string File { get; set; } = default!;
        public bool Success { get; set; }   
        public IList<Exception> Errors { get; } 
        public bool RequireRestart { get; set; }
    }
}
