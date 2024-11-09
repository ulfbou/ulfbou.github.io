namespace Homepage.Common.Models;

public class SkillModel
{
    public string Name { get; set; }
    public int Level { get; set; }
    public string Tooltip { get; set; }
    public ICollection<string> Category { get; set; }
    public string Relevance { get; set; }

    public IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            Name = "C#";
            yield return "Name is required";
        }

        if (Level < 1 || Level > 10)
        {
            Level = 3;
            yield return "Level is required";
        }

        if (string.IsNullOrWhiteSpace(Tooltip))
        {
            Tooltip = "C# is a general-purpose, multi-paradigm programming language encompassing strong typing, lexically scoped, imperative, declarative, functional, generic, object-oriented, and component-oriented programming disciplines.";
            yield return "Tooltip is required";
        }

        if (Category == null || !Category.Any())
        {
            Category = ["Programming Language"];
            yield return "Category is required";
        }

        if (string.IsNullOrWhiteSpace(Relevance))
        {
            Relevance = "High";
            yield return "Relevance is required";
        }
    }
}