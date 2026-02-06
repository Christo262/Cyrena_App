using Cyrena.Models;

namespace Cyrena.Extensions
{
    public static class ProjectFolderExtensions
    {
        public static ProjectFolder CreateFolder(this ProjectPlan plan, string id, string name)
        {
            var ext = plan.Folders.FirstOrDefault(x => x.Id == id);
            var path = Path.Combine(plan.RootDirectory, name);
            if(!Directory.Exists(path)) 
                Directory.CreateDirectory(path);
            if(ext != null)
                return ext;
            var folder = new ProjectFolder()
            {
                Id = id,
                Name = name,
                RelativePath = name
            };
            plan.Folders.Add(folder);
            return folder;
        }

        public static ProjectFolder CreateFolder(this ProjectPlan plan, ProjectFolder folder, string id, string name)
        {
            var ext = folder.Folders.FirstOrDefault(x => x.Id == id);
            var path = Path.Combine(plan.RootDirectory, folder.Name, name);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            if (ext != null)
                return ext;
            var model = new ProjectFolder()
            {
                Id = id,
                Name = name,
                RelativePath = Path.Combine(folder.RelativePath, name)
            };
            folder.Folders.Add(model);
            return model;
        }

        public static bool RemoveFolder(this ProjectPlan plan, ProjectFolder folder, bool recursive = false)
        {
            var path = Path.Combine(plan.RootDirectory, folder.RelativePath);
            if(Directory.Exists(path))
                Directory.Delete(path, recursive);

            var easy = plan.Folders.FirstOrDefault(x => x.Id == folder.Id);
            if(easy != null)
            {
                plan.Folders.Remove(easy);
                return true;
            }

            foreach(var item in plan.Folders)
            {
                if (plan.TryRemoveFolder(item, folder.Id))
                    return true;
            }
            return false;
        }

        private static bool TryRemoveFolder(this ProjectPlan plan, ProjectFolder from, string folderId)
        {
            var tl = from.Folders.FirstOrDefault(x => x.Id == folderId);
            if(tl != null)
            {
                from.Folders.Remove(tl);
                return true;
            }

            foreach( var folder in from.Folders)
            {
                if(plan.TryRemoveFolder(folder, folderId))
                    return true;
            }
            return false;
        }

        public static bool TryFindFolder(this ProjectPlan plan,  string folderId, out ProjectFolder? folder, bool recursive = true)
        {
            var f = plan.Folders.FirstOrDefault(x => x.Id ==folderId);
            if(f != null)
            {
                folder = f;
                return true;
            }

            if(recursive)
            foreach(var item in plan.Folders)
                if (item.TryFindFolder(folderId, out folder)) return true;

            folder = null;
            return false;
        }

        public static bool TryFindFolder(this ProjectFolder folder, string folderId, out ProjectFolder? model, bool recursive = true)
        {
            var f = folder.Folders.FirstOrDefault(x => x.Id == folderId);
            if(f != null)
            {
                model = f;
                return true;
            }

            if(recursive)
            foreach(var item in folder.Folders)
                if (item.TryFindFolder(folderId, out model)) 
                    return true;

            model = null;
            return false;
        }

        public static ProjectFolder? GetFolderOfFile(this ProjectPlan plan, ProjectFile file)
        {
            foreach(var item in plan.Folders)
            {
                var fld = plan.GetFolderOfFile(item, file);
                if (fld != null) return fld;
            }
            return null;
        }

        public static ProjectFolder? GetFolderOfFile(this ProjectPlan plan, ProjectFolder folder, ProjectFile file)
        {
            var ext = folder.Files.FirstOrDefault(x => x.Id == file.Id);
            if (ext != null)
                return folder;

            foreach(var item in folder.Folders)
            {
                var fld = plan.GetFolderOfFile(item, file);
                if (fld != null) return fld;
            }
            return null;
        }

        public static ProjectFolder GetOrCreateFolder(this ProjectPlan plan, string id, string name)
        {
            if (!plan.TryFindFolder(id, out var folder, false))
                return plan.CreateFolder(id, name);
            return folder!;
        }

        public static ProjectFolder GetOrCreateFolder(this ProjectPlan plan, ProjectFolder parent, string id, string name)
        {
            if (!plan.TryFindFolder(id, out var folder))
                return plan.CreateFolder(parent, id, name);
            return folder!;
        }
    }
}
