namespace Homepage.Common.Models;

public class EducationModel
{
    public string Degree { get; set; }
    public string Institution { get; set; }
    public string GraduationDate { get; set; }
    public string? OrganisationLogoUrl { get; set; }

    public IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Degree))
        {
            yield return "Degree is required";
        }

        if (string.IsNullOrWhiteSpace(Institution))
        {
            yield return "Institution is required";
        }

        if (GraduationDate == default)
        {
            yield return "GraduationDate is required";
        }
    }
}