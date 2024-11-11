using Markdig;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

using MudBlazor;

using Serilog;

using System.Net.Http.Json;

using Homepage.Common.Models;
using Homepage.Common.Services;

using Homepage.Common.Helpers;

namespace Homepage.Components.Base;

public abstract class ContentBase : BaseComponent
{
    private readonly ContentContext _contentContext;
    private readonly NavigationManager _navigationManager;

    private void OnNavigationLocationChanged(object sender, LocationChangedEventArgs e)
    {
        // Implement logic to activate/deactivate the context based on the new URL
        // For example, you could use URL patterns or specific routes to trigger context changes.
        // Here's a simplified example:

        if (e.Location.Contains("/category/devops"))
        {
            _contentContext.AddCategories(new[] { "DevOps", "Cloud", ".NET" });
        }
        else if (e.Location.Contains("/category/web"))
        {
            _contentContext.AddCategories(new[] { "Web Development", "Frontend", "Backend" });
        }
        else
        {
            _contentContext.Clear();
        }
    }
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    [Inject] protected ContentService ContentService { get; set; }

    protected List<ContentItem> ContentList { get; set; } = new List<ContentItem>();
    protected string ContentHtml { get; set; }
    protected bool IsLoading { get; set; } = false;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    protected string MetadataUrl => "content/metadata.json";
    protected string ContentDirectory => "content";

    protected override async Task OnInitializedAsync()
    {
        await LoadMetadata();
    }

    // Load JSON metadata with notifications and loading indicator
    protected async Task LoadMetadata()
    {
        var logger = Log.ForContext("Class: {Name}", GetType().Name).ForContext("Method", "LoadMetadata");
        if (string.IsNullOrEmpty(MetadataUrl))
        {
            logger.Error("MetadataUrl is null or empty.");
            Snackbar.Add("Failed to load content metadata.", Severity.Error);
            return;
        }

        try
        {
            IsLoading = true;
            logger.Information("Loading metadata from: {MetadataUrl}", MetadataUrl);
            ContentList = await Http.GetFromJsonAsync<List<ContentItem>>(MetadataUrl) ?? throw new InvalidOperationException();
            logger.Information("Loaded {Count} content items.", ContentList.Count);
            SortMetadata();
            logger.Information("Sorted content items by relevance.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to load content metadata.");
            Snackbar.Add($"Failed to load content", Severity.Error);
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void SortMetadata()
    {
        // Sort the content according to relevance from Categories: DevOps, Cloud and .NET
        // Compare with Jaccard similarity coefficient
        var categories = new List<string> { "DevOps", "Cloud", ".NET" };
        HashSet<string> baseCategoryTags = GetTagsByCategory(categories);

        ContentList = ContentList.OrderByDescending(contentItem => Similarity.CalculateJaccard(baseCategoryTags, contentItem.Tags.ToHashSet())).ToList();
    }

    // Get all tags associated with a list of category names
    private HashSet<string> GetTagsByCategory(List<string> categoryNames)
    {
        var categoryNamesJoined = string.Join("|", categoryNames);
        var tagCollection = new HashSet<string>();
        var categories = ContentList.Select(selector => selector.Tags.Where(tag => tag.Contains(categoryNamesJoined)));

        foreach (var categoryName in categoryNames)
        {
            var tags = GetTagsByCategory(categoryName);
            tagCollection.UnionWith(tags);
        }
        return tagCollection;
    }

    private HashSet<string> GetTagsByCategory(string categoryName)
    {
        return ContentList.FirstOrDefault(c => c.Title.Contains(categoryName))?.Tags.ToHashSet() ?? new HashSet<string>();
    }

    // Load Markdown content with notifications and loading indicator
    protected async Task LoadContent(string contentFileName)
    {
        var logger = Log.ForContext("Class: {Name}", GetType().Name).ForContext("Method", "LoadContent");
        try
        {
            IsLoading = true;
            var url = $"{ContentDirectory}/{contentFileName}.md";
            logger.Information("Loading content from: {Url}", url);
            var markdown = await Http.GetStringAsync(url);
            logger.Information("Loaded content from: {Url}", url);
            ContentHtml = Markdown.ToHtml(markdown, new MarkdownPipelineBuilder().Build());
            logger.Information("Converted markdown to HTML.");
        }
        catch (Exception ex)
        {
            logger.Error(ex, "Failed to load content from: {Url}", contentFileName);
            Snackbar.Add($"Failed to load content: {ex.Message}", Severity.Error);
            throw;
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected void ResetContent()
    {
        ContentHtml = string.Empty;
    }

    private double CompareContentItems<T>(ContentItem item1, T relation, string propertyName) where T : IEnumerable<string>
    {
        var item1Relations = item1.GetType().GetProperty(propertyName)?.GetValue(item1) as IEnumerable<string>;
        if (item1Relations == null)
        {
            return 0; // Handle case where property is missing or null
        }

        var relationSet = new HashSet<string>(relation);
        return Similarity.CalculateJaccard(item1Relations.ToHashSet(), relationSet);
    }
}