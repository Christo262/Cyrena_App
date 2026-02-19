using Cyrena.Developer.Models;

namespace Cyrena.Developer.Contracts
{
    /// <summary>
    /// For different .net project types, i.e. class library, blazor, mvc, etc.
    /// </summary>
    public interface IDotnetProjectType
    {
        /// <summary>
        /// The project type, ex. cs-class-library
        /// </summary>
        string Id { get; }

        /// <summary>
        /// i.e. .NET C# Class Library
        /// </summary>
        string ProjectTypeName { get; }

        /// <summary>
        /// Check if the project is handled by this implementation. Should try to detect class-lib, blazor, etc.
        /// </summary>
        /// <param name="info"></param>
        /// <returns></returns>
        bool IsOfType(ProjectInfo info);

        DevelopPlan IndexPlan(ProjectModel model);
    }
}
