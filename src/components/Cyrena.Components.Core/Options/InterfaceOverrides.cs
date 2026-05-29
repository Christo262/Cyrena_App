using Cyrena.Contracts;

namespace Cyrena.Options
{
    /// <summary>
    /// Used to change default components in the chat interface. Kernel Locked
    /// </summary>
    public sealed class InterfaceOverrides
    {
        private Type? _fileAttacher { get; set; }

        public void UseFileAttacher<TFileAttacher>()
            where TFileAttacher : class, IFileAttacher
        {
            _fileAttacher = typeof(TFileAttacher);
        }

        public Type? FileAttacher => _fileAttacher;
    }
}
