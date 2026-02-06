using Cyrena.Contracts;
using Cyrena.Models;
using Cyrena.Spec.Contracts;
using Cyrena.Spec.Models;
using Microsoft.SemanticKernel;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Text;

namespace Cyrena.Spec.Plugins
{
    internal class SpecsPlugin
    {
        private readonly ISpecsService _specs;
        public SpecsPlugin(ISpecsService specs)
        {
            _specs = specs;
        }

        [KernelFunction]
        [Description("Search Project Specifications for authoritative technical documentation about this project. Use this before implementing features to understand APIs, architecture rules, integration contracts, and established behavior.")]
        public IEnumerable<ArticleSummary> SearchProjectSpecifications(
            [Description("Keywords describing what specification you are looking for (interfaces, services, architecture, styling, integration, etc.).")] string[] keywords,
            [Description("Maximum number of results to return. Default 10.")] int maxResults = 10)
        {
            return _specs.Search(keywords, maxResults);
        }

        [KernelFunction]
        [Description("Read a Project Specification document. These documents contain grounded technical specifications about real project code and represent authoritative implementation knowledge.")]
        public string ReadProjectSpecification(
            [Description("The id of the specification document to read.")] string id)
        {
            return _specs.Read(id);
        }

        [KernelFunction]
        [Description(@"Create a new Project Specification document.

A Project Specification is authoritative technical documentation grounded in actual source code.

When creating a specification about code:
1. Read all relevant source files first.
2. Base the document only on real implementation.
3. Never write generic or hypothetical descriptions.
4. Capture real method signatures, contracts, architecture rules, and usage patterns.
5. The purpose is to guide future implementation accurately.

This document becomes authoritative project knowledge.")]
        public ToolResult<NewArticle> CreateProjectSpecification(
            [Description("Title of the specification document.")] string title,
            [Description("Keywords used to search for this specification in the future.")] string[] keywords,
            [Description("Brief summary of what the specification contains.")] string summary,
            [Description("Grounded technical content in plaintext or markdown. Do not include Title, Summary or Keywords here.")] string content)
        {
            return _specs.Create(title, keywords, summary, content);
        }

        [KernelFunction]
        [Description(@"Update an existing Project Specification.

Updates must remain grounded in source code.
If implementation changes, the specification must be revised to match reality.")]
        public ToolResult<NewArticle> UpdateProjectSpecification(
            [Description("The id of the specification to update.")] string id,
            [Description("New title. Leave null if unchanged.")] string? title = null,
            [Description("Updated search keywords. Leave null if unchanged.")] string[]? keywords = null,
            [Description("Updated summary. Leave null if unchanged.")] string? summary = null,
            [Description("Updated grounded technical content. Do not include Title, Summary or Keywords here. Leave null if unchanged.")] string? content = null)
        {
            return _specs.Update(id, title, keywords, summary, content);
        }

        [KernelFunction]
        [Description("Delete a Project Specification that is obsolete or incorrect. Only delete specifications that are no longer valid.")]
        public ToolResult DeleteProjectSpecification(
            [Description("The id of the specification to delete.")] string id)
        {
            return _specs.Delete(id);
        }
    }

}
