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
            og!.Lines[index] = line;
            var content = og.ToString();
            File.WriteAllText(Path.Combine(plan.RootDirectory, file.RelativePath), content);
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
    }
}
