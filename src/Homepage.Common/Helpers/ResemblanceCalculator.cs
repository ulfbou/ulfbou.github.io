namespace Homepage.Common.Helpers
{
    public class ResemblanceCalculator<TCategory> where TCategory : notnull, IEquatable<TCategory>
    {
        private readonly Dictionary<TCategory, Dictionary<string, double>> _categoryTagWeights;

        public ResemblanceCalculator(Dictionary<TCategory, Dictionary<string, double>> categoryTagWeights)
        {
            _categoryTagWeights = categoryTagWeights;
        }

        public double CalculateResemblance(TCategory category1, TCategory category2)
        {
            var tags1 = _categoryTagWeights[category1];
            var tags2 = _categoryTagWeights[category2];

            // Calculate the weighted intersection
            double weightedIntersection = tags1.Keys.Intersect(tags2.Keys)
                .Sum(tag => tags1[tag] * tags2[tag]);

            // Calculate the weighted union
            double weightedUnion = tags1.Values.Sum() + tags2.Values.Sum() - weightedIntersection;

            return weightedIntersection / weightedUnion;
        }

        public static double JaccardSimilarity(HashSet<string> set1, HashSet<string> set2)
        {
            var intersectionCount = set1.Intersect(set2).Count();
            var unionCount = set1.Union(set2).Count();
            return (double)intersectionCount / unionCount;
        }

        public static List<(string Category, double Similarity)> SortCategoriesBySimilarity(Dictionary<string, HashSet<string>> categoryTags, string targetCategory)
        {
            var targetTags = categoryTags[targetCategory];
            var sortedCategories = new List<(string Category, double Similarity)>();

            foreach (var category in categoryTags.Keys)
            {
                if (category != targetCategory)
                {
                    var similarity = JaccardSimilarity(targetTags, categoryTags[category]);
                    sortedCategories.Add((category, similarity));
                }
            }

            sortedCategories.Sort((a, b) => b.Similarity.CompareTo(a.Similarity));
            return sortedCategories;
        }
    }
}
