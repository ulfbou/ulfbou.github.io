namespace Homepage.Common.Models;

public class AchievementModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Date { get; set; }

    public IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            Name = "Achievement";
            yield return "Name is required";
        }

        if (string.IsNullOrWhiteSpace(Description))
        {
            Description = "Description";
            yield return "Description is required";
        }

        if (Date == default)
        {
            yield return "Date is required";
        }
    }
}