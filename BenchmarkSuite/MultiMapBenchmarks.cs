using BenchmarkDotNet.Attributes;
using MultiMap.Entities;
using MultiMap.Helpers;
using Microsoft.VSDiagnostics;

[CPUUsageDiagnoser]
public class MultiMapBenchmarks
{
    private MultiMapSet<string, int> _setMap = null!;
    private MultiMapList<string, int> _listMap = null!;
    private ConcurrentMultiMap<string, int> _concurrentMap = null!;
    private SortedMultiMap<string, int> _sortedMap = null!;
    private MultiMapSet<string, int> _helperTarget = null!;
    private MultiMapSet<string, int> _helperOther = null!;
    private const int KeyCount = 100;
    private const int ValuesPerKey = 50;

    [GlobalSetup]
    public void Setup()
    {
        _setMap = new MultiMapSet<string, int>();
        _listMap = new MultiMapList<string, int>();
        _concurrentMap = new ConcurrentMultiMap<string, int>();
        _sortedMap = new SortedMultiMap<string, int>();
        for (int k = 0; k < KeyCount; k++)
        {
            string key = $"key{k}";
            for (int v = 0; v < ValuesPerKey; v++)
            {
                _setMap.Add(key, v);
                _listMap.Add(key, v);
                _concurrentMap.Add(key, v);
                _sortedMap.Add(key, v);
            }
        }

        _helperTarget = new MultiMapSet<string, int>();
        _helperOther = new MultiMapSet<string, int>();
        for (int k = 0; k < 50; k++)
        {
            for (int v = 0; v < 20; v++)
            {
                _helperTarget.Add($"key{k}", v);
                _helperOther.Add($"key{k + 25}", v + 10);
            }
        }
    }

    // --- Count benchmarks (the O(k) hot path) ---
    [Benchmark]
    public int MultiMapSet_Count() => _setMap.Count;

    [Benchmark]
    public int MultiMapList_Count() => _listMap.Count;

    [Benchmark]
    public int ConcurrentMultiMap_Count() => _concurrentMap.Count;

    [Benchmark]
    public int SortedMultiMap_Count() => _sortedMap.Count;

    // --- Add benchmarks ---
    [Benchmark]
    public void MultiMapSet_Add()
    {
        var map = new MultiMapSet<string, int>();
        for (int k = 0; k < KeyCount; k++)
        {
            string key = $"key{k}";
            for (int v = 0; v < ValuesPerKey; v++)
            {
                map.Add(key, v);
            }
        }
    }

    [Benchmark]
    public void MultiMapList_Add()
    {
        var map = new MultiMapList<string, int>();
        for (int k = 0; k < KeyCount; k++)
        {
            string key = $"key{k}";
            for (int v = 0; v < ValuesPerKey; v++)
            {
                map.Add(key, v);
            }
        }
    }

    // --- Get benchmarks ---
    [Benchmark]
    public int MultiMapSet_Get()
    {
        int sum = 0;
        for (int k = 0; k < KeyCount; k++)
        {
            foreach (var v in _setMap.Get($"key{k}"))
                sum += v;
        }

        return sum;
    }

    // --- Contains benchmarks ---
    [Benchmark]
    public bool MultiMapSet_Contains() => _setMap.Contains("key50", 25);
    // --- Helper Intersect benchmark ---
    [Benchmark]
    public void MultiMapHelper_Intersect()
    {
        var target = new MultiMapSet<string, int>();
        var other = new MultiMapSet<string, int>();
        for (int k = 0; k < 50; k++)
        {
            for (int v = 0; v < 20; v++)
            {
                target.Add($"key{k}", v);
                other.Add($"key{k + 25}", v + 10);
            }
        }

        target.Intersect(other);
    }
}