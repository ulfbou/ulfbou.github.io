using Homepage.Common.Models;

namespace Homepage.Common.Helpers;

public class ContentContext
{
    public IEnumerable<string> Categories { get => _categories; }
    private List<string> _categories = new List<string>();

    public IEnumerable<string> Tags { get => _tags; }
    private List<string> _tags = new List<string>();

    public IEnumerable<string> Keywords { get => _keywords; }
    private List<string> _keywords = new List<string>();

    public void AddCategories(IEnumerable<string> categories)
    {
        ArgumentNullException.ThrowIfNull(categories, nameof(categories));

        _categories.AddRange(categories);
    }

    public void AddTags(IEnumerable<string> tags)
    {
        ArgumentNullException.ThrowIfNull(tags, nameof(tags));

        _tags.AddRange(tags);
    }

    public void AddKeywords(IEnumerable<string> keywords)
    {
        ArgumentNullException.ThrowIfNull(keywords, nameof(keywords));

        _keywords.AddRange(keywords);
    }

    public bool RemoveCategories(IEnumerable<string> categories)
    {
        ArgumentNullException.ThrowIfNull(categories, nameof(categories));

        foreach (var category in categories)
        {
            _categories.Remove(category);
        }

        return true;
    }

    public bool RemoveTags(IEnumerable<string> tags)
    {
        ArgumentNullException.ThrowIfNull(tags, nameof(tags));

        foreach (var tag in tags)
        {
            _tags.Remove(tag);
        }

        return true;
    }

    public bool RemoveKeywords(IEnumerable<string> keywords)
    {
        ArgumentNullException.ThrowIfNull(keywords, nameof(keywords));

        foreach (var keyword in keywords)
        {
            _keywords.Remove(keyword);
        }

        return true;
    }

    public void Clear()
    {
        _categories.Clear();
        _tags.Clear();
        _keywords.Clear();
    }
}