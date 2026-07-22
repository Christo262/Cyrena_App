using System.Text.Json.Serialization;

namespace Cyrena.Coding.Models;

public abstract class FileTypeAllowance
{
    public List<string> AllowedFileTypes { get; set; } = [];
    [JsonIgnore]
    public List<string> IgnoredDirectories { get; set; } = [".cyrena"];

    public List<string> ReadOnlyFileTypes { get; set; } = [];

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

    public void AddIgnoredDirectory(string directory)
    {
        if(!IgnoredDirectories.Contains(directory))
            IgnoredDirectories.Add(directory);
    }

    public void RemoveIgnoredDirectory(string directory)
    {
        if (IgnoredDirectories.Contains(directory))
            IgnoredDirectories.Remove(directory);
    }
}

public abstract class FileTypeAllowanceDevelopItem : DevelopItem
{
    public List<string> AllowedFileTypes { get; set; } = [];
    public List<string> ReadOnlyFileTypes { get; set; } = [];

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