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
        void SetTransparent(bool b);
        void SetFullScreen(bool b);
        Task<string[]> ShowFileSelect(string title, string name, string[] filters);
        Task<string?> ShowSaveFile(string title, string name, string[] filters);

        void SetHeight(int h);
        void SetWidth(int w);

        void SetTitle(string title);
    }
}
