using Cyrena.Developer.Models;

namespace Cyrena.Developer.Extensions
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
            var path = Path.Combine(plan.RootDirectory, fileName);
            if (!File.Exists(path))
                File.WriteAllText(path, content);
            if (ext != null)
                return ext;
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
            var path = Path.Combine(plan.RootDirectory, folder.RelativePath, fileName);
            if (!File.Exists(path))
                File.WriteAllText(path, content);
            if (ext != null)
                return ext;
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
                plan.RemoveFile(file);
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
                plan.RemoveFile(file);
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

        public static bool TryWriteFileLine(this DevelopPlan plan, DevelopFile file, int index, string line, out DevelopFileLines? lines)
        {
            if (!plan.TryReadFileLines(file, out var og))
            {
                lines = null;
                return false;
            }

            // Validate index
            if (index < 0 || index >= og!.Lines.Count)
            {
                lines = null;
                return false;
            }

            og.Lines[index] = line;
            var content = og.ToString();
            var path = Path.Combine(plan.RootDirectory, file.RelativePath);

            File.WriteAllText(path, content);
            lines = og;
            return true;
        }

        /// <summary>
        /// Tries to insert a line at <paramref name="index"/> in <paramref name="file"/>.
        /// Returns true and the updated <see cref="DevelopFileLines"/> on success,
        /// otherwise false (and <c>lines</c> is null).
        /// </summary>
        public static bool TryInsertLine(
            this DevelopPlan plan,
            DevelopFile file,
            int index,
            string line,
            out DevelopFileLines? lines)
        {
            // 1️⃣ Read the current file lines
            if (!plan.TryReadFileLines(file, out var original))
            {
                lines = null;
                return false;
            }

            var og = original!; // TryReadFileLines succeeded, so not null

            // 2️⃣ Validate the index (insertion allowed at the end)
            if (index < 0 || index > og.Lines.Count)
            {
                lines = null;
                return false;
            }

            // 3️⃣ Build a new dictionary with the line inserted
            var newLines = new Dictionary<int, string>();

            foreach (var kvp in og.Lines.OrderBy(k => k.Key))
            {
                // Shift down every line that is at or after the insertion point
                int newKey = kvp.Key >= index ? kvp.Key + 1 : kvp.Key;
                newLines[newKey] = kvp.Value;
            }

            // Insert the new line
            newLines[index] = line;

            // Replace the original collection
            og.Lines = newLines;

            // 4️⃣ Write the updated content back to the file
            var path = Path.Combine(plan.RootDirectory, file.RelativePath);
            try
            {
                File.WriteAllText(path, og.ToString()); // ToString() joins with \r\n
            }
            catch
            {
                lines = null;
                return false;
            }

            // 5️⃣ Return the updated object
            lines = og;
            return true;
        }


        public static void IndexFiles(this DevelopPlan plan, DevelopFolder folder, string extension, string id_prefix, bool readOnly = false)
        {
            var cmp_path = Path.Combine(plan.RootDirectory, folder.RelativePath);

            var files = Directory.GetFiles(cmp_path, $"*.{extension}");
            foreach (var file in files)
            {
                var info = new FileInfo(file);
                var name = info.Name.Replace($".{extension}", "");
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
            var files = Directory.GetFiles(plan.RootDirectory, $"*.{extension}");
            foreach (var file in files)
            {
                var info = new FileInfo(file);
                var name = info.Name.Replace($".{extension}", "");
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

        public static bool TryReplaceLines(
    this DevelopPlan plan,
    DevelopFile file,
    int startIndex,          // zero‑based line that begins the range
    int count,               // how many existing lines to remove
    IEnumerable<string> replacement, // new lines that will take their place
    out DevelopFileLines? lines)
        {
            // 1️⃣  Load the current lines
            if (!plan.TryReadFileLines(file, out var original))
            {
                lines = null;
                return false;
            }

            var og = original!;
            var total = og.Lines.Count;

            if (startIndex < 0 || startIndex > total)
            {
                lines = null;
                return false;
            }

            if (count < 0)                                     // negative count makes no sense
            {
                lines = null;
                return false;
            }

            var effectiveCount = Math.Min(count, total - startIndex);

            var newLines = new Dictionary<int, string>();
            int newKey = 0;

            // a) lines before the range
            foreach (var kvp in og.Lines.OrderBy(k => k.Key).Take(startIndex))
            {
                newLines[newKey++] = kvp.Value;
            }

            // b) replacement lines
            foreach (var repl in replacement)
            {
                newLines[newKey++] = repl;
            }

            // c) lines after the removed range – they need to be shifted by
            //    (replacement.Count - effectiveCount)
            int shift = replacement.Count() - effectiveCount;
            foreach (var kvp in og.Lines.OrderBy(k => k.Key).Skip(startIndex + effectiveCount))
            {
                newLines[newKey++] = kvp.Value;
            }

            og.Lines = newLines;
            var path = Path.Combine(plan.RootDirectory, file.RelativePath);
            try
            {
                File.WriteAllText(path, og.ToString());   // ToString() joins with \r\n
            }
            catch
            {
                lines = null;
                return false;
            }

            lines = og;
            return true;
        }
    }
}
