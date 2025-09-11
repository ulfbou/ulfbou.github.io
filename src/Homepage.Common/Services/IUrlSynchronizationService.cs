
namespace Homepage.Common.Services
{
    public interface IUrlSynchronizationService
    {
        string? ParseContentSlugFromUrl();
        (string SearchText, IEnumerable<string> ActiveTags, string Audience)? ParseFiltersFromUrl();
        void SynchronizeContentToUrl(string? slug);
        void SynchronizeFiltersToUrl(string searchText, IEnumerable<string> activeTags, string audience);
    }
}