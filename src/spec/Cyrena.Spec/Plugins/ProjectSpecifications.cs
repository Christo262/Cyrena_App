using Cyrena.Models;
using Cyrena.Spec.Contracts;
using Cyrena.Spec.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Cyrena.Spec.Plugins
{
    internal class ProjectSpecifications
    {
        private readonly ISpecsService _specs;
        public ProjectSpecifications(ISpecsService specs)
        {
            _specs = specs;
        }

        [KernelFunction("list_all")]
        [Description("List All Project Specifications for authoritative technical documentation about this project. Use this before implementing features to understand APIs, architecture rules, integration contracts, and established behavior.")]
        public IEnumerable<ArticleSummary> ListProjectSpecifications()
        {
            var models = _specs.Articles.Select(x => new ArticleSummary(x.Id, x.Title, x.Summary));
            return models;
        }

        [KernelFunction("search")]
        [Description("Search Project Specifications for authoritative technical documentation about this project. Use this before implementing features to understand APIs, architecture rules, integration contracts, and established behavior.")]
        public async Task<IEnumerable<ArticleSummary>> SearchProjectSpecifications(
            [Description("Keywords describing what specification you are looking for (interfaces, services, architecture, styling, integration, etc.).")] string[] keywords,
            [Description("Maximum number of results to return. Default 10.")] int maxResults = 10)
        {
            return await _specs.Search(keywords, maxResults);
        }

        [KernelFunction("read")]
        [Description("Read a Project Specification document. These documents contain grounded technical specifications about real project code and represent authoritative implementation knowledge.")]
        public async Task<string> ReadProjectSpecification(
            [Description("The id of the specification document to read.")] string id)
        {
            return await _specs.Read(id);
        }

        [KernelFunction("create")]
        [Description(@"Create a new Project Specification document.

A Project Specification is authoritative technical documentation grounded in actual source code.

When creating a specification about code:
1. Read all relevant source files first.
2. Base the document only on real implementation.
3. Never write generic or hypothetical descriptions.
4. Capture real method signatures, contracts, architecture rules, and usage patterns.
5. The purpose is to guide future implementation accurately.

This document becomes authoritative project knowledge.")]
        public async Task<ToolResult<NewArticle>> CreateProjectSpecification(
            [Description("Title of the specification document. Mandatory.")] string title,
            [Description("Keywords used to search for this specification in the future. Mandatory.")] string[] keywords,
            [Description("Brief summary of what the specification contains. Mandatory.")] string summary,
            [Description("Grounded technical content in plaintext or markdown. Do not include Title, Summary or Keywords here. Mandatory.")] string content,
            [Description("If the specification is related directly to a file, provide the fileId for linkage. Optional.")] string? fileId = null)
        {
            if(!string.IsNullOrEmpty(fileId))
                return await _specs.CreateOrUpdateForFile(fileId, title, keywords, summary, content);
            return await _specs.Create(title, keywords, summary, content);
        }

        [KernelFunction("update")]
        [Description(@"Update an existing Project Specification.

Updates must remain grounded in source code.
If implementation changes, the specification must be revised to match reality.")]
        public async Task<ToolResult<NewArticle>> UpdateProjectSpecification(
            [Description("The id of the specification to update. Mandatory.")] string id,
            [Description("New title. Leave null or empty if unchanged. Optional.")] string? title = null,
            [Description("Updated search keywords. Leave null if unchanged. Optional.")] string[]? keywords = null,
            [Description("Updated summary. Leave null or empty if unchanged. Optional.")] string? summary = null,
            [Description("Updated grounded technical content. Do not include Title, Summary or Keywords here. Leave null or empty if unchanged. Optional.")] string? content = null)
        {
            return await _specs.Update(id, title, keywords, summary, content);
        }

        [KernelFunction("delete")]
        [Description("Permanently delete a Project Specification that is obsolete or incorrect. Only delete specifications that are no longer valid.")]
        public async Task<ToolResult> DeleteProjectSpecification(
            [Description("The id of the specification to delete.")] string id)
        {
            return await _specs.Delete(id);
        }
    }

}
