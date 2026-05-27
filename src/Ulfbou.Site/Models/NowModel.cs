namespace Ulfbou.Site.Models;
public class NowModel { public string SchemaVersion { get; set; } = "1"; public DateTime GeneratedAtUtc { get; set; } public OwnerInfo Owner { get; set; } = new(); public SiteInfo Site { get; set; } = new(); public CurrentFocus CurrentFocus { get; set; } = new(); public PulseInfo Pulse { get; set; } = new(); public Signals Signals { get; set; } = new(); }
public class OwnerInfo { public string Name { get; set; } = ""; public string Github { get; set; } = ""; public string Location { get; set; } = ""; }
public class SiteInfo { public string Title { get; set; } = ""; public string Mode { get; set; } = ""; }
public class CurrentFocus { public string Summary { get; set; } = ""; public string PrimaryRepo { get; set; } = ""; public List<string> SecondaryRepos { get; set; } = new(); }
public class PulseInfo { public string CurrentWeek { get; set; } = ""; public string FriendlyMarkdown { get; set; } = ""; public string TechnicalMetrics { get; set; } = ""; }
public class Signals { public int ActiveRepos { get; set; } public int OpenLoops { get; set; } public int StalePullRequests { get; set; } public int QuietRepos { get; set; } }
