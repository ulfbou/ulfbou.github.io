namespace Homepage.Common.Services;

public class RandomizationService
{
    private static readonly Random _random = new Random();
    private bool _isGridLayout;
    private bool _isCompact;
    private bool _isTimeLine;
    private bool _isAccordion;

    public RandomizationService()
    {
        Randomize();
    }

    public void Randomize()
    {
        _isGridLayout = _random.Next(0, 2) == 0;
        _isCompact = _random.Next(0, 2) == 0;
        _isTimeLine = _random.Next(0, 2) == 0;
        _isAccordion = _random.Next(0, 2) == 0;
    }

    public bool IsGridLayout() => _isGridLayout;
    public bool IsCompact() => _isCompact;
    public bool IsTimeline() => _isTimeLine;
    public bool IsAccordion() => _isAccordion;

    public enum CvSectionType
    {
        PersonalInformation,
        WorkExperience,
        Education,
        Skills,
        Projects,
        Certifications,
        Languages,
        Hobbies
    }
}