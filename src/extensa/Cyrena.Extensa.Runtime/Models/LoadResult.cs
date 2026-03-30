namespace Cyrena.Extensa.Loader.Models
{
    public class LoadResult
    {
        public bool Success { get; set; }
        public bool RequireRestart { get; set; }
        public List<Exception> Errors { get; set; } = new List<Exception>();
    }
}
