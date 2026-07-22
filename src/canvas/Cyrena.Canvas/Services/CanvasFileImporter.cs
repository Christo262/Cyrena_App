using Cyrena.Canvas.Options;
using Cyrena.Contracts;
using Cyrena.Models;
using MudBlazor;

namespace Cyrena.Canvas.Services
{
    internal class CanvasFileImporter : ICyrenaFileImporter
    {
        private readonly IWindowLauncher _launcher;
        public CanvasFileImporter(IWindowLauncher launcher)
        {
            _launcher = launcher;
        }

        public string Id => CanvasOptions.ImporterId;
        public Task ImportAsync(CyrenaFileManifest manifest, string absoluteDataPath, CancellationToken cancellationToken = default)
        {
            var entry = manifest[CanvasOptions.Entry];
            if (string.IsNullOrEmpty(entry))
                throw new Exception("Unable to determine entry");
            var entryFile = Path.Combine(absoluteDataPath, entry);
            if (!File.Exists(entryFile))
                throw new Exception("Invalid entry defined in manifest");
            _launcher.Show($"file://{entryFile}", 800, 800);
            return Task.CompletedTask;
        }
    }
}
