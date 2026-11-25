using FsCheckProperty = FsCheck.NUnit.PropertyAttribute;
using MultiMap.Entities;

namespace MultiMap.Tests;

[TestFixture]
public class MultiMapPropertyTests
{
    [FsCheckProperty]
    public bool AddThenContainsProperty(int key, int value)
    {
        var map = new MultiMapSet<int, int>();

        return map.Add(key, value) && map.Contains(key, value);
    }

    [FsCheckProperty]
    public bool CountMatchesDistinctValuesProperty(int key, int[] values)
    {
        var map = new MultiMapSet<int, int>();
        var distinctValues = new HashSet<int>();

        foreach (int value in values)
        {
            map.Add(key, value);
            distinctValues.Add(value);
        }

        return map.Count == distinctValues.Count
            && map.GetValuesCount(key) == distinctValues.Count;
    }

    [FsCheckProperty]
    public bool RemoveRangePreservesRemainingPairsProperty(int key, int[] values)
    {
        var map = new MultiMapList<int, int>();
        foreach (int value in values)
        {
            map.Add(key, value);
        }

        var pairs = values
            .Distinct()
            .Select(value => new KeyValuePair<int, int>(key, value))
            .ToArray();

        int removed = map.RemoveRange(pairs);
        int distinctCount = values.Distinct().Count();
        int remainingCount = values.Length - distinctCount;

        return removed == distinctCount
            && map.Count == remainingCount
            && map.ContainsKey(key) == (remainingCount > 0);
    }

    [FsCheckProperty]
    public bool RepeatedRemoveRangeInputRemovesEachPairOnceProperty(int key, int[] values)
    {
        var map = new MultiMapSet<int, int>();
        foreach (int value in values)
        {
            map.Add(key, value);
        }

        var pairs = values
            .Distinct()
            .SelectMany(value => new[]
            {
                new KeyValuePair<int, int>(key, value),
                new KeyValuePair<int, int>(key, value)
            })
            .ToArray();

        int removed = map.RemoveRange(pairs);

        return removed == values.Distinct().Count()
            && map.Count == 0;
    }
}
