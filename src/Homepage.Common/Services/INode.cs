namespace Homepage.Common.Services
{
    public interface INode
    {
        string Name { get; }
        IEnumerable<string> GetElements();
    }
}