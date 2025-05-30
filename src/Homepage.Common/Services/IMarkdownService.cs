// --- Services/MarkdownService.cs ---
using Homepage.Common.Models;

using Microsoft.AspNetCore.Components;

namespace Homepage.Common.Services
{
    public interface IMarkdownService
    {
        /// <summary>
        /// Fetches the content metadata (e.g., blog post details) from a remote JSON file.
        /// Caches the data in local storage for performance.
        /// </summary>
        /// <returns>A list of ContentMetadata objects.</returns>
        Task<List<ContentMetadata>> GetContentMetadataAsync();

        /// <summary>
        /// Fetches the raw Markdown content for a given path.
        /// Caches the content in local storage using a stale-while-revalidate pattern.
        /// </summary>
        /// <param name="contentPath">The relative path to the Markdown file (e.g., "my-post.md").</param>
        /// <returns>The raw Markdown content as a string.</returns>
        Task<string> GetMarkdownContentAsync(string contentPath);

        /// <summary>
        /// Renders a Markdown string into an HTML string.
        /// Uses Markdig with advanced extensions.
        /// </summary>
        /// <param name="markdown">The Markdown string to render.</param>
        /// <returns>The rendered HTML string.</returns>
        Task<string> RenderMarkdownToHtmlAsync(string markdown);
    }
}
