namespace Ulfbou.Portfolio.Core;

public sealed record PortfolioData(Profile Profile, IReadOnlyList<Project> Projects, IReadOnlyList<string> Facets);
public sealed record Profile(string Name, string Signal, string Introduction, string Direction, IReadOnlyList<Link> Links);
public sealed record Project(
    string Id, string Title, string ShortTitle, string Proposition, string Story,
    string Contribution, string Reflection, string Status, string Accent,
    IReadOnlyList<string> Facets, IReadOnlyList<string> Technologies,
    IReadOnlyList<Proof> Proof, Demo Demo, IReadOnlyList<string> Limitations);
public sealed record Proof(string Label, string Value);
public sealed record Demo(string Kind, string Caption, IReadOnlyList<string> Lines);
public sealed record Link(string Label, string Url);
