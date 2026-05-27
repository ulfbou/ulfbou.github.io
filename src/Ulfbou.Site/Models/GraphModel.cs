namespace Ulfbou.Site.Models;
public class GraphModel { public string SchemaVersion { get; set; } = "1"; public List<GraphNode> Nodes { get; set; } = new(); public List<GraphEdge> Edges { get; set; } = new(); }
public class GraphNode { public string Id { get; set; } = ""; public string Label { get; set; } = ""; public string Group { get; set; } = ""; }
public class GraphEdge { public string Source { get; set; } = ""; public string Target { get; set; } = ""; public string Type { get; set; } = ""; }
