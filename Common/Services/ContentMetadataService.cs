public class ContentMetadataService
{
    private readonly Dictionary<string, ContentMetadata> _contentMetadataStore = new();

    public void InitializeContentData(IEnumerable<ContentMetadata> initialData)
    {
        foreach (var content in initialData)
        {
            _contentMetadataStore[content.ContentId] = content;
        }
    }

    public ContentMetadata? GetContentById(string contentId) =>
        _contentMetadataStore.TryGetValue(contentId, out var content) ? content : null;

    public IEnumerable<ContentMetadata> GetContentBySection(string section) =>
        _contentMetadataStore.Values.Where(c => c.Section == section);

    public IEnumerable<ContentMetadata> GetContentByCategory(string category) =>
        _contentMetadataStore.Values.Where(c => c.Categories.Any(cat => cat.Name == category));

    public IEnumerable<ContentMetadata> GetContentByTag(string tag) =>
        _contentMetadataStore.Values.Where(c => c.Tags.Contains(tag));

    public IEnumerable<ContentMetadata> GetContentByKeyword(string keyword) =>
        _contentMetadataStore.Values.Where(c => c.Keywords.Contains(keyword));

    public IEnumerable<ContentMetadata> GetContentByAuthor(string author) =>
        _contentMetadataStore.Values.Where(c => c.Author == author);

    public IEnumerable<ContentMetadata> GetContentByTargetAudience(string audience) =>
        _contentMetadataStore.Values.Where(c => c.TargetAudience == audience);

    public IEnumerable<ContentMetadata> GetContentByContentFormat(string format) =>
        _contentMetadataStore.Values.Where(c => c.ContentFormat == format);

    public IEnumerable<ContentMetadata> GetContentByContentLength(int minLength, int maxLength) =>
        _contentMetadataStore.Values.Where(c => c.ContentLength >= minLength && c.ContentLength <= maxLength);

    public void UpdateContent(ContentMetadata updatedContent)
    {
        if (_contentMetadataStore.ContainsKey(updatedContent.ContentId))
        {
            _contentMetadataStore[updatedContent.ContentId] = updatedContent;
        }
    }
}