using BenchmarkDotNet.Attributes;
using MultiMap.Entities;
using MultiMap.Helpers;
using Microsoft.VSDiagnostics;
using BenchmarkSuite;

[CPUUsageDiagnoser]
public class MultiMapBenchmarks
{
    private MultiMapSet<string, int> _setMap = null!;
    private MultiMapList<string, int> _listMap = null!;
    private ConcurrentMultiMap<string, int> _concurrentMap = null!;
    private SortedMultiMap<string, int> _sortedMap = null!;
    private MultiMapSet<string, int> _helperTarget = null!;
    private MultiMapSet<string, int> _helperOther = null!;

    [GlobalSetup]
    public void Setup()
    {
        _setMap = new MultiMapSet<string, int>();
        _listMap = new MultiMapList<string, int>();
        _concurrentMap = new ConcurrentMultiMap<string, int>();
        _sortedMap = new SortedMultiMap<string, int>();
        for (int k = 0; k < Consts.KeyCount; k++)
        {
            string key = $"key{k}";
            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                _setMap.Add(key, v);
                _listMap.Add(key, v);
                _concurrentMap.Add(key, v);
                _sortedMap.Add(key, v);
            }
        }

        _helperTarget = new MultiMapSet<string, int>();
        _helperOther = new MultiMapSet<string, int>();
        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                _helperTarget.Add($"key{k}", v);
                _helperOther.Add($"key{k + Consts.KeyOffset}", v + Consts.ValueOffset);
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
        for (int k = 0; k < Consts.KeyCount; k++)
        {
            string key = $"key{k}";
            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.Add(key, v);
            }
        }
    }

    [Benchmark]
    public void MultiMapList_Add()
    {
        var map = new MultiMapList<string, int>();
        for (int k = 0; k < Consts.KeyCount; k++)
        {
            string key = $"key{k}";
            for (int v = 0; v < Consts.ValuesPerKey; v++)
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
        for (int k = 0; k < Consts.KeyCount; k++)
        {
            foreach (var v in _setMap.Get($"key{k}"))
                sum += v;
        }

        return sum;
    }

    // --- Contains benchmarks ---
    [Benchmark]
    public bool MultiMapSet_Contains() => _setMap.Contains("key50", 25);

    // --- Helper set-operation benchmarks (MultiMapSet) ---
    [Benchmark]
    public void MultiMapHelper_Union()
    {
        var target = new MultiMapSet<string, int>();
        var other = new MultiMapSet<string, int>();
        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"key{k}", v);
                other.Add($"key{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.Union(other);
    }

    [Benchmark]
    public void MultiMapHelper_Intersect()
    {
        var target = new MultiMapSet<string, int>();
        var other = new MultiMapSet<string, int>();
        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"key{k}", v);
                other.Add($"key{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.Intersect(other);
    }

    [Benchmark]
    public void MultiMapHelper_ExceptWith()
    {
        var target = new MultiMapSet<string, int>();
        var other = new MultiMapSet<string, int>();
        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"key{k}", v);
                other.Add($"key{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.ExceptWith(other);
    }

    [Benchmark]
    public void MultiMapHelper_SymmetricExceptWith()
    {
        var target = new MultiMapSet<string, int>();
        var other = new MultiMapSet<string, int>();
        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"key{k}", v);
                other.Add($"key{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.SymmetricExceptWith(other);
    }

    // --- ConcurrentMultiMap set-operation benchmarks ---
    [Benchmark]
    public void ConcurrentMultiMap_Union()
    {
        var target = new ConcurrentMultiMap<string, int>();
        var other = new ConcurrentMultiMap<string, int>();
        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"key{k}", v);
                other.Add($"key{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.Union(other);
    }

    [Benchmark]
    public void ConcurrentMultiMap_Intersect()
    {
        var target = new ConcurrentMultiMap<string, int>();
        var other = new ConcurrentMultiMap<string, int>();
        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"key{k}", v);
                other.Add($"key{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.Intersect(other);
    }

    [Benchmark]
    public void ConcurrentMultiMap_ExceptWith()
    {
        var target = new ConcurrentMultiMap<string, int>();
        var other = new ConcurrentMultiMap<string, int>();
        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"key{k}", v);
                other.Add($"key{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.ExceptWith(other);
    }

    [Benchmark]
    public void ConcurrentMultiMap_SymmetricExceptWith()
    {
        var target = new ConcurrentMultiMap<string, int>();
        var other = new ConcurrentMultiMap<string, int>();
        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"key{k}", v);
                other.Add($"key{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.SymmetricExceptWith(other);
    }

    // --- SortedMultiMap set-operation benchmarks ---
    [Benchmark]
    public void SortedMultiMap_Union()
    {
        var target = new SortedMultiMap<string, int>();
        var other = new SortedMultiMap<string, int>();
        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"key{k}", v);
                other.Add($"key{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.Union(other);
    }

    [Benchmark]
    public void SortedMultiMap_Intersect()
    {
        var target = new SortedMultiMap<string, int>();
        var other = new SortedMultiMap<string, int>();
        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"key{k}", v);
                other.Add($"key{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.Intersect(other);
    }

    [Benchmark]
    public void SortedMultiMap_ExceptWith()
    {
        var target = new SortedMultiMap<string, int>();
        var other = new SortedMultiMap<string, int>();
        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"key{k}", v);
                other.Add($"key{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.ExceptWith(other);
    }

    [Benchmark]
    public void SortedMultiMap_SymmetricExceptWith()
    {
        var target = new SortedMultiMap<string, int>();
        var other = new SortedMultiMap<string, int>();
        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"key{k}", v);
                other.Add($"key{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.SymmetricExceptWith(other);
    }

    // --- MultiMapList set-operation benchmarks ---
    [Benchmark]
    public void MultiMapList_Union()
    {
        var target = new MultiMapList<string, int>();
        var other = new MultiMapList<string, int>();
        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"key{k}", v);
                other.Add($"key{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.Union(other);
    }

    [Benchmark]
    public void MultiMapList_Intersect()
    {
        var target = new MultiMapList<string, int>();
        var other = new MultiMapList<string, int>();
        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"key{k}", v);
                other.Add($"key{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.Intersect(other);
    }

    [Benchmark]
    public void MultiMapList_ExceptWith()
    {
        var target = new MultiMapList<string, int>();
        var other = new MultiMapList<string, int>();
        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"key{k}", v);
                other.Add($"key{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.ExceptWith(other);
    }

    [Benchmark]
    public void MultiMapList_SymmetricExceptWith()
    {
        var target = new MultiMapList<string, int>();
        var other = new MultiMapList<string, int>();
        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"key{k}", v);
                other.Add($"key{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.SymmetricExceptWith(other);
    }
}