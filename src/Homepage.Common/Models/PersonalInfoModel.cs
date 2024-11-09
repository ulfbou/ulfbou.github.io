namespace Homepage.Common.Models;

public class PersonalInfoModel
{
    public string Name { get; set; }
    public string Title { get; set; }
    public string Location { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
    public string LinkedIn { get; set; }
    public string ProfileImageUrl { get; set; }

    public IEnumerable<string> Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
        {
            Name = "Ulf Bourelius";
            yield return "Name is required";
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            Title = "Fullstack .NET Software Developer";
            yield return "Title is required";
        }

        if (string.IsNullOrWhiteSpace(Location))
        {
            Location = "Stockholm, Sweden";
            yield return "Location is required";
        }

        if (string.IsNullOrWhiteSpace(Email))
        {
            Email = "ulfbourelius71@gmail.com";
            yield return "Email is required";
        }

        if (string.IsNullOrWhiteSpace(Phone))
        {
            Phone = "+4670-1234567";
            yield return "Phone is required";
        }

        if (string.IsNullOrWhiteSpace(LinkedIn))
        {
            LinkedIn = "https://www.linkedin.com/in/ulf-bourelius-0b1b1b1b1/";
            yield return "LinkedIn is required";
        }

        if (string.IsNullOrWhiteSpace(ProfileImageUrl))
        {
            ProfileImageUrl = "https://avatars.githubusercontent.com/u/1234567?v=4";
            yield return "ProfileImageUrl is required";
        }
    }
}
