﻿using Microsoft.JSInterop;

namespace Ulfbou.GitHub.IO.Common.Services;

public class Similarity
{
    public static double CalculateJaccard(HashSet<string> set1, HashSet<string> set2)
    {
        var intersection = set1.Intersect(set2);
        var union = set1.Union(set2);
        var similarity = union.Count() == 0 ? 0.0 : (double)intersection.Count() / union.Count();

        return similarity;
    }
}