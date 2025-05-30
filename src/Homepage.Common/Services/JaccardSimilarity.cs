namespace Homepage.Common.Helpers
{
    public class Similarity
    {
        public static double CalculateJaccard(HashSet<string> set1, HashSet<string> set2)
        {
            if (!set1.Any() && !set2.Any()) return 1.0;
            if (!set1.Any() || !set2.Any()) return 0.0;

            var intersection = set1.Intersect(set2);
            var union = set1.Union(set2);
            var similarity = (double)intersection.Count() / union.Count();
            return similarity;
        }
    }
}
