using Cyrena.Coding.Models;

namespace Cyrena.Coding.Contracts
{
    /// <summary>
    /// Capped, timestamped in-memory version history for files modified by AI.
    /// Each file tracks up to <c>MaxVersionsPerFile</c> snapshots (oldest dropped first).
    /// </summary>
    public interface IVersionControl
    {
        /// <summary>Maximum number of versions retained per file. Oldest are dropped when exceeded.</summary>
        int MaxVersionsPerFile { get; set; }

        // ── Write ────────────────────────────────────────────────────────────

        /// <summary>Records a new version snapshot. No-op if <paramref name="file"/> is null.</summary>
        void Backup(DevelopFileContent? file, string? label = null);

        /// <summary>Removes all version history for the given file.</summary>
        void RemoveBackup(string fileId);

        /// <summary>Clears all version history for every file.</summary>
        void Clear();

        // ── Query ────────────────────────────────────────────────────────────

        /// <summary>Returns true if any versions exist for the given file.</summary>
        bool HasBackup(string fileId);

        /// <summary>Returns the most recent version for a file, or null if none.</summary>
        DevelopFileVersion? GetLatest(string fileId);

        /// <summary>
        /// Returns the full ordered version history for a file (oldest → newest).
        /// Returns an empty list when no history exists.
        /// </summary>
        IReadOnlyList<DevelopFileVersion> GetHistory(string fileId);

        /// <summary>Returns the latest version for every tracked file.</summary>
        IEnumerable<DevelopFileVersion> GetAllLatest();

        // ── Restore ──────────────────────────────────────────────────────────

        /// <summary>
        /// Retrieves the version at <paramref name="index"/> (0 = oldest) for the given file.
        /// Returns false when the file has no history or the index is out of range.
        /// </summary>
        bool TryGetVersion(string fileId, int index, out DevelopFileVersion? version);

        /// <summary>
        /// Retrieves the version whose timestamp is closest to (but not after)
        /// <paramref name="at"/>. Returns false if no suitable version exists.
        /// </summary>
        bool TryGetVersionAt(string fileId, DateTimeOffset at, out DevelopFileVersion? version);

        // ── Rollback ─────────────────────────────────────────────────────────

        /// <summary>
        /// Rolls back to <paramref name="version"/>, removing all versions that came after it.
        /// Returns the version rolled back to, or null if the version could not be found in history.
        /// </summary>
        DevelopFileVersion? RollbackTo(DevelopFileVersion version);

        /// <summary>
        /// Rolls back one step to the previous version, removing the current latest.
        /// Returns the version rolled back to, or null if there is nothing to roll back to.
        /// </summary>
        DevelopFileVersion? RollbackOne(string fileId);

        // ── Backward-compatible shims (mirrors original interface) ────────────

        /// <summary>Returns the content of the most recent backup, or null. (Compat shim.)</summary>
        DevelopFileContent? GetBackups(string fileId);

        /// <summary>Returns the content of the most recent backup for every tracked file. (Compat shim.)</summary>
        IEnumerable<DevelopFileContent> GetBackups();
    }
}