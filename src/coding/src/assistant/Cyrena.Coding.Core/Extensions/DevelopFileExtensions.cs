using Cyrena.Coding.Models;

namespace Cyrena.Coding.Extensions
{
    public static class DevelopFileExtensions
    {
        /// <summary>
        /// Creates a file in the root directory
        /// </summary>
        /// <param name="plan"></param>
        /// <param name="fileId"></param>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static DevelopFile CreateFile(this DevelopPlan plan, string fileId, string fileName, string? content)
        {
            var ext = plan.Files.FirstOrDefault(f => f.Id == fileId);
            if (ext != null)
            {
                var extPath = Path.Combine(plan.RootDirectory, ext.RelativePath);
                if (!File.Exists(extPath))
                    File.WriteAllText(extPath, content);
                return ext;
            }
            var path = Path.Combine(plan.RootDirectory, fileName);
            if (!File.Exists(path))
                File.WriteAllText(path, content);
            var model = new DevelopFile()
            {
                Id = fileId,
                Name = fileName,
                RelativePath = fileName,
            };
            plan.Files.Add(model);
            return model;
        }

        /// <summary>
        /// Creates a file in a folder
        /// </summary>
        /// <param name="plan"></param>
        /// <param name="folder"></param>
        /// <param name="fileId"></param>
        /// <param name="fileName"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public static DevelopFile CreateFile(this DevelopPlan plan, DevelopFolder folder, string fileId, string fileName, string? content)
        {
            var ext = folder.Files.FirstOrDefault(f => f.Id == fileId);
            if (ext != null)
            {
                var extPath = Path.Combine(plan.RootDirectory, ext.RelativePath);
                if (!File.Exists(extPath))
                    File.WriteAllText(extPath, content);
                return ext;
            }

            if (folder.IsVirtual)
            {
                var folderPath = Path.Combine(plan.RootDirectory, folder.RelativePath);
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);
                folder.IsVirtual = false;
            }
            var path = Path.Combine(plan.RootDirectory, folder.RelativePath, fileName);
            if (!File.Exists(path))
                File.WriteAllText(path, content);
            var model = new DevelopFile()
            {
                Id = fileId,
                Name = fileName,
                RelativePath = Path.Combine(folder.RelativePath, fileName),
            };
            folder.Files.Add(model);
            return model;
        }

        public static bool TryReadFileContent(this DevelopPlan plan, DevelopFile file, out DevelopFileContent? content)
        {
            var path = Path.Combine(plan.RootDirectory, file.RelativePath);
            if (!File.Exists(path))
            {
                content = null;
                return false;
            }
            var text = File.ReadAllText(path);
            content = new DevelopFileContent(file, text);
            return true;
        }

        public static bool TryReadFileLines(this DevelopPlan plan, DevelopFile file, out DevelopFileLines? lines)
        {
            var path = Path.Combine(plan.RootDirectory, file.RelativePath);
            if (!File.Exists(path))
            {
                lines = null;
                return false;
            }
            var text = File.ReadAllText(path);
            lines = new DevelopFileLines(file, text);
            return true;
        }

        public static bool RemoveFile(this DevelopPlan plan, DevelopFile file)
        {
            var easy = plan.Files.FirstOrDefault(x => x.Id == file.Id);
            if (easy != null)
            {
                var path = Path.Combine(plan.RootDirectory, easy.RelativePath);
                if (File.Exists(path))
                    File.Delete(path);
                plan.Files.Remove(easy);
                return true;
            }

            foreach (var item in plan.Folders)
            {
                if (plan.RemoveFile(item, file))
                    return true;
            }
            return false;
        }

        public static bool RemoveFile(this DevelopPlan pl, DevelopFolder folder, DevelopFile file)
        {
            var easy = folder.Files.FirstOrDefault(x => x.Id == file.Id);
            if (easy != null)
            {
                var path = Path.Combine(pl.RootDirectory, easy.RelativePath);
                if (File.Exists(path))
                    File.Delete(path);
                folder.Files.Remove(easy);
                return true;
            }

            foreach (var item in folder.Folders)
            {
                if (pl.RemoveFile(item, file))
                    return true;
            }
            return false;
        }

        public static bool TryFindFile(this DevelopPlan plan, string fileId, out DevelopFile? file, bool recursive = true)
        {
            var easy = plan.Files.FirstOrDefault(x => x.Id == fileId);
            if (easy != null)
            {
                file = easy;
                return true;
            }

            if (recursive)
                foreach (var item in plan.Folders)
                {
                    if (plan.TryFindFile(item, fileId, out file))
                    {
                        return true;
                    }
                }
            file = null;
            return false;
        }

        public static bool TryFindFile(this DevelopPlan pl, DevelopFolder folder, string fileId, out DevelopFile? file, bool recursive = true)
        {
            var easy = folder.Files.FirstOrDefault(y => y.Id == fileId);
            if (easy != null)
            {
                file = easy;
                return true;
            }

            if (recursive)
                foreach (var item in folder.Folders)
                {
                    if (pl.TryFindFile(item, fileId, out var flf))
                    {
                        file = flf;
                        return true;
                    }
                }
            file = null;
            return false;
        }

        public static bool TryWriteFileContent(this DevelopPlan plan, DevelopFile file, string? content, out DevelopFileContent? fileContent)
        {
            var path = Path.Combine(plan.RootDirectory, file.RelativePath);
            if (!File.Exists(path))
            {
                fileContent = null;
                return false;
            }

            File.WriteAllText(path, content);
            fileContent = new DevelopFileContent(file, content);
            return true;
        }


        public static void IndexFiles(this DevelopPlan plan, DevelopFolder folder, string extension, string id_prefix, bool readOnly = false)
        {
            if (readOnly) folder.AddReadOnlyFile(extension);
            else folder.AddAllowedFile(extension);
            
            var cmp_path = Path.Combine(plan.RootDirectory, folder.RelativePath);
            if(!Directory.Exists(cmp_path))
                return;
            var files = Directory.GetFiles(cmp_path, $"*.{extension}");
            foreach (var file in files)
            {
                var info = new FileInfo(file);
                var suffix = $".{extension}";
                var name = info.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                    ? info.Name[..^suffix.Length]
                    : info.Name;
                var id = $"{id_prefix}{name}";
                if (!plan.TryFindFile(folder, id, out var _, false))
                {
                    var model = new DevelopFile()
                    {
                        Id = id,
                        Name = info.Name,
                        RelativePath = Path.Combine(folder.RelativePath, info.Name),
                        ReadOnly = readOnly
                    };
                    folder.Files.Add(model);
                }
            }

            folder.Files.RemoveAll(f =>
                    !File.Exists(Path.Combine(plan.RootDirectory, f.RelativePath)));
        }

        public static void IndexFiles(this DevelopPlan plan, string extension, string id_prefix, bool readOnly = false)
        {
            if (readOnly) plan.AddReadOnlyFile(extension);
            else plan.AddAllowedFile(extension);
            
            var files = Directory.GetFiles(plan.RootDirectory, $"*.{extension}");
            foreach (var file in files)
            {
                var info = new FileInfo(file);
                var suffix = $".{extension}";
                var name = info.Name.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)
                    ? info.Name[..^suffix.Length]
                    : info.Name;
                var id = $"{id_prefix}{name}";
                if (!plan.TryFindFile(id, out var _, false))
                {
                    var model = new DevelopFile()
                    {
                        Id = id,
                        Name = info.Name,
                        RelativePath = info.Name,
                        ReadOnly = readOnly
                    };
                    plan.Files.Add(model);
                }
            }

            plan.Files.RemoveAll(f =>
                !File.Exists(Path.Combine(plan.RootDirectory, f.RelativePath)));
        }

        public static bool TryFindFileByName(this DevelopPlan plan, string name, out DevelopFile? file, bool recursive = true)
        {
            var easy = plan.Files.FirstOrDefault(x =>
                string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            if (easy != null)
            {
                file = easy;
                return true;
            }

            if (recursive)
                foreach (var item in plan.Folders)
                {
                    if (plan.TryFindFileByName(item, name, out file))
                        return true;
                }

            file = null;
            return false;
        }

        public static bool TryFindFileByName(this DevelopPlan plan, DevelopFolder folder, string name, out DevelopFile? file, bool recursive = true)
        {
            var easy = folder.Files.FirstOrDefault(x =>
                string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase));

            if (easy != null)
            {
                file = easy;
                return true;
            }

            if (recursive)
                foreach (var item in folder.Folders)
                {
                    if (plan.TryFindFileByName(item, name, out file))
                        return true;
                }

            file = null;
            return false;
        }

        /// <summary>
        /// Overwrites the entire file with new content.
        /// </summary>
        public static bool TryWriteFileOverwrite(
            this DevelopPlan plan,
            DevelopFile file,
            string? content,
            out DevelopFileLines? lines)
        {
            return plan.TryWriteFileContentAsLines(file, content, out lines);
        }

        /// <summary>
        /// Replaces a range of lines (0-based startLine, lineCount lines) with new content.
        /// Passing null or empty content deletes the lines without inserting anything.
        /// </summary>
        public static bool TryWriteFileReplace(
            this DevelopPlan plan,
            DevelopFile file,
            string? content,
            int startLine,
            int lineCount,
            out DevelopFileLines? lines,
            out int? totalLines)
        {
            if (!plan.TryReadFileLines(file, out var current) || current == null)
            {
                lines = null;
                totalLines = null;
                return false;
            }

            var existingLines = current.Lines
                .OrderBy(x => x.Index)
                .Select(x => x.Text ?? string.Empty)
                .ToList();

            totalLines = existingLines.Count;

            if (startLine < 0 || lineCount <= 0 || startLine + lineCount > totalLines)
            {
                lines = null;
                return false;
            }

            existingLines.RemoveRange(startLine, lineCount);

            var incomingLines = SplitIncomingLines(content);
            if (incomingLines.Count > 0)
                existingLines.InsertRange(startLine, incomingLines);

            var updatedContent = string.Join("\n", existingLines);
            return plan.TryWriteFileContentAsLines(file, updatedContent, out lines);
        }

        /// <summary>
        /// Inserts content before the specified line (0-based). Use startLine == existingLines.Count to append.
        /// </summary>
        public static bool TryWriteFileInsert(
            this DevelopPlan plan,
            DevelopFile file,
            string? content,
            int startLine,
            out DevelopFileLines? lines,
            out int? totalLines)
        {
            if (!plan.TryReadFileLines(file, out var current) || current == null)
            {
                lines = null;
                totalLines = null;
                return false;
            }

            var existingLines = current.Lines
                .OrderBy(x => x.Index)
                .Select(x => x.Text ?? string.Empty)
                .ToList();

            totalLines = existingLines.Count;

            // Allow startLine == totalLines to support appending after the last line
            if (startLine < 0 || startLine > totalLines)
            {
                lines = null;
                return false;
            }

            var incomingLines = SplitIncomingLines(content);
            existingLines.InsertRange(startLine, incomingLines);

            var updatedContent = string.Join("\n", existingLines);
            return plan.TryWriteFileContentAsLines(file, updatedContent, out lines);
        }

        private static bool TryWriteFileContentAsLines(
            this DevelopPlan plan,
            DevelopFile file,
            string? content,
            out DevelopFileLines? lines)
        {
            var path = Path.Combine(plan.RootDirectory, file.RelativePath);

            if (!File.Exists(path))
            {
                lines = null;
                return false;
            }

            try
            {
                var normalizedContent = NormalizeLineEndings(content ?? string.Empty);
                File.WriteAllText(path, normalizedContent);
                lines = new DevelopFileLines(file, normalizedContent);
                return true;
            }
            catch
            {
                lines = null;
                return false;
            }
        }

        private static List<string> SplitIncomingLines(string? content)
        {
            if (string.IsNullOrEmpty(content))
                return new List<string>();

            return NormalizeLineEndings(content)
                .Split('\n', StringSplitOptions.None)
                .ToList();
        }

        private static string NormalizeLineEndings(string content)
        {
            return content
                .Replace("\r\n", "\n")
                .Replace("\r", "\n");
        }

        public static bool TryFindFileByPath(this DevelopPlan plan, string relativePath, out DevelopFile? file, bool recursive = true)
        {
            var normalized = relativePath.Replace('\\', '/').TrimStart('/');

            var easy = plan.Files.FirstOrDefault(x =>
                x.RelativePath.Replace('\\', '/').TrimStart('/').Equals(normalized, StringComparison.OrdinalIgnoreCase));

            if (easy != null) { file = easy; return true; }

            if (recursive)
                foreach (var folder in plan.Folders)
                    if (plan.TryFindFileByPath(folder, normalized, out file))
                        return true;

            file = null;
            return false;
        }

        public static bool TryFindFileByPath(this DevelopPlan plan, DevelopFolder folder, string relativePath, out DevelopFile? file, bool recursive = true)
        {
            var normalized = relativePath.Replace('\\', '/').TrimStart('/');

            var easy = folder.Files.FirstOrDefault(x =>
                x.RelativePath.Replace('\\', '/').TrimStart('/').Equals(normalized, StringComparison.OrdinalIgnoreCase));

            if (easy != null) { file = easy; return true; }

            if (recursive)
                foreach (var child in folder.Folders)
                    if (plan.TryFindFileByPath(child, normalized, out file))
                        return true;

            file = null;
            return false;
        }
    }
}