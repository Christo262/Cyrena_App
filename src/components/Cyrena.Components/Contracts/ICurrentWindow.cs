using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Contracts
{
    /// <summary>
    /// Controls the current window
    /// </summary>
    public interface ICurrentWindow : IDisposable
    {
        void Minimize();
        void Close();
        void Maximize();

        void Restore();
    }
}
