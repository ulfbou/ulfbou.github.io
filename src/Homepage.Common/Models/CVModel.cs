namespace Homepage.Common.Models;

public class CVModel
{
    public PersonalInfoModel PersonalInfo { get; set; } = new PersonalInfoModel();
    public List<WorkExperienceModel> WorkExperiences { get; set; } = new List<WorkExperienceModel>();
    public List<EducationModel> Educations { get; set; } = new List<EducationModel>();
    public List<SkillModel> Skills { get; set; } = new List<SkillModel>();
    public List<ProjectModel> Projects { get; set; } = new List<ProjectModel>();
    public List<AchievementModel> Achievements { get; set; } = new List<AchievementModel>();

    public IEnumerable<string> Validate()
    {
        if (PersonalInfo == null)
        {
            yield return "PersonalInfo is required";
        }
        else
        {
            foreach (var error in PersonalInfo.Validate())
            {
                yield return error;
            }
        }

        if (WorkExperiences == null || WorkExperiences.Count == 0)
        {
            yield return "At least one WorkExperience is required";
        }
        else
        {
            foreach (var workExperience in WorkExperiences)
            {
                foreach (var error in workExperience.Validate())
                {
                    yield return error;
                }
            }
        }

        if (Educations == null || Educations.Count == 0)
        {
            yield return "At least one Education is required";
        }
        else
        {
            foreach (var education in Educations)
            {
                foreach (var error in education.Validate())
                {
                    yield return error;
                }
            }
        }

        if (Skills == null || Skills.Count == 0)
        {
            yield return "At least one Skill is required";
        }
        else
        {
            foreach (var skill in Skills)
            {
                foreach (var error in skill.Validate())
                {
                    yield return error;
                }
            }
        }

        if (Projects == null || Projects.Count == 0)
        {
            yield return "At least one Project is required";
        }
        else
        {
            foreach (var project in Projects)
            {
                foreach (var error in project.Validate())
                {
                    yield return error;
                }
            }
        }

        if (Achievements == null || Achievements.Count == 0)
        {
            yield return "At least one Achievement is required";
        }
        else
        {
            foreach (var achievement in Achievements)
            {
                foreach (var error in achievement.Validate())
                {
                    yield return error;
                }
            }
        }
    }
}