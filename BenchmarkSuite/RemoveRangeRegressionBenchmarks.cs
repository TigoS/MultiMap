using BenchmarkDotNet.Attributes;
using MultiMap.Entities;

namespace BenchmarkSuite;

[MemoryDiagnoser]
public class RemoveRangeRegressionBenchmarks
{
    private KeyValuePair<string, int>[] _pairs = null!;

    [Params(100, 1000)]
    public int PairCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _pairs = new KeyValuePair<string, int>[PairCount];
        for (int i = 0; i < PairCount; i++)
        {
            _pairs[i] = new KeyValuePair<string, int>($"key{i % 100}", i);
        }
    }

    [Benchmark(Baseline = true)]
    public int ItemByItemBaseline()
    {
        var map = CreatePopulatedMap();
        int removed = 0;

        foreach (var pair in _pairs)
        {
            if (map.Remove(pair.Key, pair.Value))
            {
                removed++;
            }
        }

        return removed;
    }

    [Benchmark]
    public int BatchedRemoveRange()
    {
        var map = CreatePopulatedMap();
        return map.RemoveRange(_pairs);
    }

    private MultiMapSet<string, int> CreatePopulatedMap()
    {
        var map = new MultiMapSet<string, int>(PairCount);
        foreach (var pair in _pairs)
        {
            map.Add(pair.Key, pair.Value);
        }

        return map;
    }
}
