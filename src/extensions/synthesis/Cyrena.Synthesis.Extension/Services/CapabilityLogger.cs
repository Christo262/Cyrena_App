using Cyrena.Synthesis.Contracts;
using Cyrena.Synthesis.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cyrena.Synthesis.Services
{
    internal class CapabilityLogger : ICapabilityLogger
    {
        private readonly StringBuilder _outputBuffer;
        private readonly string _scriptId;
        private readonly Action<LogEntry>? _onLog;

        public string CapturedOutput => _outputBuffer.ToString();

        public CapabilityLogger(ICapabilityContext ctx, Action<LogEntry>? onLog = null)
        {
            _scriptId = ctx.Current?.Id ?? string.Empty;
            _outputBuffer = new StringBuilder();
            _onLog = onLog;
        }

        public void Debug(string message)
        {
            Log("DEBUG", message);
        }

        public void Info(string message)
        {
            Log("INFO", message);
        }

        public void Warn(string message)
        {
            Log("WARN", message);
        }

        public void Error(string message)
        {
            Log("ERROR", message);
        }

        public void Error(string message, Exception exception)
        {
            Log("ERROR", $"{message} | Exception: {exception.GetType().Name}: {exception.Message}");
        }

        private void Log(string level, string message)
        {
            var entry = new LogEntry
            {
                Timestamp = DateTime.UtcNow,
                Level = level,
                ScriptId = _scriptId,
                Message = message
            };

            var formatted = $"[{entry.Timestamp:HH:mm:ss.fff}] [{level}] {message}";
            _outputBuffer.AppendLine(formatted);
            _onLog?.Invoke(entry);
        }
    }
}
