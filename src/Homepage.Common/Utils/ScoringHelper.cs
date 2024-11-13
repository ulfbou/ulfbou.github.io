using Homepage.Common.Models;

namespace Homepage.Common.Utils;

public static class ScoringHelper
{
    public static int ScoreByTagMatch(TaggableItem item, HashSet<string> referenceTags)
    {
        var sharedTags = item.Tags.Intersect(referenceTags);
        return sharedTags.Count();
    }

    public static int ScoreByKeywordMatch(TaggableItem item, HashSet<string> referenceKeywords)
    {
        var sharedKeywords = item.Keywords.Intersect(referenceKeywords);
        return sharedKeywords.Count();
    }
}