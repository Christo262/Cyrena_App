namespace Cyrena.Contracts
{
    public interface IFileDialog
    {
        /// <summary>
        /// Shows an open file dialog with the specified title and file filter.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="ftr">Optional filter: (display name, file extensions).</param>
        /// <returns>The selected file path, or null if canceled.</returns>
        Task<string?> OpenAsync(string title, (string filterName, string[] extensions)? ftr);

        /// <summary>
        /// Shows a save file dialog with the specified title, file filter, and default path.
        /// </summary>
        /// <param name="title">The title of the dialog.</param>
        /// <param name="ftr">Optional filter: (display name, file extensions).</param>
        /// <param name="defaultPath">Optional default file path.</param>
        /// <returns>The selected file path, or null if canceled.</returns>
        Task<string?> ShowSaveFileAsync(string title, (string filterName, string[] extensions)? ftr, string? defaultPath = null);

        void ExploreFolder(string folderPath);

        Task<string?> SelectFolder(string title = "Select Folder", string? current = null);
    }
}
