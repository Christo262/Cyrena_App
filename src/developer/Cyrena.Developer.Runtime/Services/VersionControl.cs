using Cyrena.Developer.Contracts;
using Cyrena.Developer.Models;

namespace Cyrena.Developer.Services
{
    /// <summary>
    /// In-memory, capped version history for files modified by AI.
    /// Thread-safe via a simple lock; suitable for single-assistant desktop use.
    /// </summary>
    internal class VersionControl : IVersionControl
    {
        // fileId → ordered history (index 0 = oldest, last = newest)
        private readonly Dictionary<string, List<DevelopFileVersion>> _history;
        private readonly object _lock = new();

        public VersionControl(int maxVersionsPerFile = 20)
        {
            _history = new Dictionary<string, List<DevelopFileVersion>>();
            MaxVersionsPerFile = maxVersionsPerFile;
        }

        /// <inheritdoc/>
        public int MaxVersionsPerFile { get; set; }

        // ── Write ────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public void Backup(DevelopFileContent? file, string? label = null)
        {
            if (file == null)
                return;

            lock (_lock)
            {
                if (!_history.TryGetValue(file.Id, out var versions))
                {
                    versions = new List<DevelopFileVersion>();
                    _history[file.Id] = versions;
                }

                versions.Add(new DevelopFileVersion(file, label));

                // Drop oldest entries when cap is exceeded
                while (MaxVersionsPerFile > 0 && versions.Count > MaxVersionsPerFile)
                    versions.RemoveAt(0);
            }
        }

        /// <inheritdoc/>
        public void RemoveBackup(string fileId)
        {
            lock (_lock)
                _history.Remove(fileId);
        }

        /// <inheritdoc/>
        public void Clear()
        {
            lock (_lock)
                _history.Clear();
        }

        // ── Query ────────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public bool HasBackup(string fileId)
        {
            lock (_lock)
                return _history.ContainsKey(fileId) && _history[fileId].Count > 0;
        }

        /// <inheritdoc/>
        public DevelopFileVersion? GetLatest(string fileId)
        {
            lock (_lock)
            {
                if (!_history.TryGetValue(fileId, out var versions) || versions.Count == 0)
                    return null;
                return versions[^1];
            }
        }

        /// <inheritdoc/>
        public IReadOnlyList<DevelopFileVersion> GetHistory(string fileId)
        {
            lock (_lock)
            {
                if (!_history.TryGetValue(fileId, out var versions))
                    return Array.Empty<DevelopFileVersion>();
                return versions.AsReadOnly();
            }
        }

        /// <inheritdoc/>
        public IEnumerable<DevelopFileVersion> GetAllLatest()
        {
            lock (_lock)
            {
                return _history.Values
                    .Where(v => v.Count > 0)
                    .Select(v => v[^1])
                    .ToList(); // materialise inside lock
            }
        }

        // ── Restore ──────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public bool TryGetVersion(string fileId, int index, out DevelopFileVersion? version)
        {
            lock (_lock)
            {
                if (!_history.TryGetValue(fileId, out var versions)
                    || index < 0
                    || index >= versions.Count)
                {
                    version = null;
                    return false;
                }
                version = versions[index];
                return true;
            }
        }

        /// <inheritdoc/>
        public bool TryGetVersionAt(string fileId, DateTimeOffset at, out DevelopFileVersion? version)
        {
            lock (_lock)
            {
                if (!_history.TryGetValue(fileId, out var versions) || versions.Count == 0)
                {
                    version = null;
                    return false;
                }

                // Walk backwards to find the newest version that is not after `at`
                for (int i = versions.Count - 1; i >= 0; i--)
                {
                    if (versions[i].Timestamp <= at)
                    {
                        version = versions[i];
                        return true;
                    }
                }

                version = null;
                return false;
            }
        }

        // ── Rollback ─────────────────────────────────────────────────────────

        /// <inheritdoc/>
        public DevelopFileVersion? RollbackTo(DevelopFileVersion version)
        {
            lock (_lock)
            {
                if (!_history.TryGetValue(version.File.Id, out var versions))
                    return null;

                var index = versions.IndexOf(version);
                if (index < 0)
                    return null;

                versions.RemoveRange(index + 1, versions.Count - index - 1);
                return versions[index];
            }
        }

        /// <inheritdoc/>
        public DevelopFileVersion? RollbackOne(string fileId)
        {
            lock (_lock)
            {
                if (!_history.TryGetValue(fileId, out var versions) || versions.Count < 2)
                    return null;

                versions.RemoveAt(versions.Count - 1);
                return versions[^1];
            }
        }

        // ── Backward-compatible shims ─────────────────────────────────────────

        /// <inheritdoc/>
        public DevelopFileContent? GetBackups(string fileId)
            => GetLatest(fileId)?.File;

        /// <inheritdoc/>
        public IEnumerable<DevelopFileContent> GetBackups()
            => GetAllLatest().Select(v => v.File);
    }
}