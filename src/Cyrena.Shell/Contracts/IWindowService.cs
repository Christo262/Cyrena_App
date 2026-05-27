using Cyrena.Options;
using System;

namespace Cyrena.Shell.Contracts
{
    public interface IWindowService : IDisposable
    {
        void Show(ApplicationOptions options);
    }
}
