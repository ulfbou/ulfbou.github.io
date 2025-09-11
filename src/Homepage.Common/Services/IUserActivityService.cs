
namespace Homepage.Common.Services
{
    public interface IUserActivityService
    {
        Task<List<string>> GetAllViewedSlugsAsync();
        Task<DateTimeOffset?> GetLastVisitTimestampAsync();
        Task<HashSet<string>> GetPinnedSlugsAsync();
        Task<List<string>> GetRecentlyViewedSlugsAsync(int count);
        Task RecordContentViewAsync(string slug);
        Task SetPinnedSlugAsync(string slug, bool isPinned);
        Task UpdateLastVisitTimestampAsync();
    }
}