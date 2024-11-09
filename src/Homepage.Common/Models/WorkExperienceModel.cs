namespace Homepage.Common.Models;

public class WorkExperienceModel
{
    public string Position { get; set; }
    public string Company { get; set; }
    public string StartDate { get; set; }
    public string EndDate { get; set; }
    public List<string> Responsibilities { get; set; }
    public string? CompanyLogoUrl { get; set; }

    public IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Position))
        {
            yield return "Position is required";
        }

        if (string.IsNullOrWhiteSpace(Company))
        {
            yield return "Company is required";
        }

        if (StartDate == default)
        {
            yield return "StartDate is required";
        }

        if (EndDate == default)
        {
            yield return "EndDate is required";
        }

        if (Responsibilities == null || Responsibilities.Count == 0)
        {
            yield return "At least one Responsibility is required";
        }
    }
}
