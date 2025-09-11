namespace Homepage.Common.Constants
{
    /// <summary>
    /// Centralized constants for application-wide configurations and magic strings.
    /// </summary>
    public static class AppConstants
    {
        // Local Storage Keys
        public const string LocalStorageLastVisitKey = "lastVisit";
        public const string LocalStorageViewedSlugsKey = "viewedSlugs";
        public const string LocalStoragePinnedSlugsKey = "pinnedSlugs";
        public const string LocalStorageContentMetadataCacheKey = "contentMetadataCache";

        public const string ContentMetadataRelativePath = "content/metadata.json";

        // Base URL for fetching raw Markdown content files from GitHub
        // This points directly to your content repository's 'main' branch for raw files.
        public const string RawGitHubContentBaseUrl = "https://raw.githubusercontent.com/ulfbou/Portfolio-Content/main/";

        // JavaScript Interop Helpers
        public const string JsLocalStorageHelperGetItem = "localStorageHelper.getItem";
        public const string JsLocalStorageHelperSetItem = "localStorageHelper.setItem";

        // Other Configuration
        public const int MaxRecentlyViewedItems = 5;

        public const string GithubContentBaseUrl = "https://ulfbou.github.io/content/";
    }
}
