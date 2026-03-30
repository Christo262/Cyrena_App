using Cyrena.Extensa.Installer.Models;

namespace Cyrena.Extensa.Installer.Contracts
{
    public interface IInstaller
    {
        InstallResult Install(string file);
    }
}
