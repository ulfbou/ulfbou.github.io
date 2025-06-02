namespace Homepage.Common.Models
{
    public class TaggableItem
    {
        public string Name { get; set; }
        public HashSet<string> Tags { get; set; }
        public HashSet<string> Keywords { get; set; }

        public TaggableItem(string name, IEnumerable<string> tags, IEnumerable<string> keywords)
        {
            Name = name;
            Tags = new HashSet<string>(tags);
            Keywords = new HashSet<string>(keywords);
        }
    }
}
