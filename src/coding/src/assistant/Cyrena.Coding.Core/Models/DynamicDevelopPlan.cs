using Cyrena.Models;

namespace Cyrena.Coding.Models;

public class DynamicDevelopPlan : Entity
{
    public List<string> AllowedFileTypes { get; set; } = [];
    public List<string> ReadOnlyFileTypes { get; set; } = [];
    public List<string> IgnoredDirectories { get; set; } = [".cyrena"];
    public List<DynamicDevelopFolder> Folders { get; set; } = [];
    
    public void AddAllowedFile(string extension)
    {
        extension = extension.ToLower().Replace(".", "");
        if (!AllowedFileTypes.Contains(extension))
            AllowedFileTypes.Add(extension);
    }
    
    public void RemoveAllowedFile(string extension)
    {
        extension = extension.ToLower().Replace(".", "");
        if (AllowedFileTypes.Contains(extension))
            AllowedFileTypes.Remove(extension);
    }
    public void AddReadOnlyFile(string extension)
    {
        extension = extension.ToLower().Replace(".", "");
        if (!ReadOnlyFileTypes.Contains(extension))
            ReadOnlyFileTypes.Add(extension);
    }
    
    public void RemoveReadOnlyFile(string extension)
    {
        extension = extension.ToLower().Replace(".", "");
        if (ReadOnlyFileTypes.Contains(extension))
            ReadOnlyFileTypes.Remove(extension);
    }
}

public class DynamicDevelopFolder : Entity
{
    public string Name { get; set; } = null!;
    public List<string> AllowedFileTypes { get; set; } = [];
    public List<string> ReadOnlyFileTypes { get; set; } = [];

    public List<DynamicDevelopFolder> Children { get; set; } = [];
    
    public void AddAllowedFile(string extension)
    {
        extension = extension.ToLower().Replace(".", "");
        if (!AllowedFileTypes.Contains(extension))
            AllowedFileTypes.Add(extension);
    }
    
    public void RemoveAllowedFile(string extension)
    {
        extension = extension.ToLower().Replace(".", "");
        if (AllowedFileTypes.Contains(extension))
            AllowedFileTypes.Remove(extension);
    }
    
    public void AddReadOnlyFile(string extension)
    {
        extension = extension.ToLower().Replace(".", "");
        if (!ReadOnlyFileTypes.Contains(extension))
            ReadOnlyFileTypes.Add(extension);
    }
    
    public void RemoveReadOnlyFile(string extension)
    {
        extension = extension.ToLower().Replace(".", "");
        if (ReadOnlyFileTypes.Contains(extension))
            ReadOnlyFileTypes.Remove(extension);
    }
}