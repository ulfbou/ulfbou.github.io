﻿using Ulfbou.GitHub.IO.Common.Models;

namespace Ulfbou.GitHub.IO.Common.Helpers
{
    public static class ContextDrivenSorter
    {
        public static List<TaggableItem> SortByContextRelevance(
            List<TaggableItem> items,
            HashSet<string> referenceTags)
        {
            return items
                .OrderByDescending(item => ScoringHelper.ScoreByTagMatch(item, referenceTags))
                .ToList();
        }
    }

}

