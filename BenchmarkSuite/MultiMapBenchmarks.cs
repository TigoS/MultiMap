using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;
using MultiMap.Entities;
using MultiMap.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace BenchmarkSuite;

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
            string key = $"{Consts.KeyPrefix}{k}";

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
                _helperTarget.Add($"{Consts.KeyPrefix}{k}", v);
                _helperOther.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
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
            string key = $"{Consts.KeyPrefix}{k}";

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
            string key = $"{Consts.KeyPrefix}{k}";

            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.Add(key, v);
            }
        }
    }

    [Benchmark]
    public void ConcurrentMultiMap_Add()
    {
        var map = new ConcurrentMultiMap<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            string key = $"{Consts.KeyPrefix}{k}";

            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.Add(key, v);
            }
        }
    }

    [Benchmark]
    public void SortedMultiMap_Add()
    {
        var map = new SortedMultiMap<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            string key = $"{Consts.KeyPrefix}{k}";

            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.Add(key, v);
            }
        }
    }

    // --- AddRange benchmarks ---
    [Benchmark]
    public void MultiMapSet_AddRange()
    {
        var map = new MultiMapSet<string, int>();
        var values = Enumerable.Range(0, Consts.ValuesPerKey).ToArray();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            map.AddRange($"{Consts.KeyPrefix}{k}", values);
        }
    }

    [Benchmark]
    public void MultiMapList_AddRange()
    {
        var map = new MultiMapList<string, int>();
        var values = Enumerable.Range(0, Consts.ValuesPerKey).ToArray();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            map.AddRange($"{Consts.KeyPrefix}{k}", values);
        }
    }

    [Benchmark]
    public void ConcurrentMultiMap_AddRange()
    {
        var map = new ConcurrentMultiMap<string, int>();
        var values = Enumerable.Range(0, Consts.ValuesPerKey).ToArray();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            map.AddRange($"{Consts.KeyPrefix}{k}", values);
        }
    }

    [Benchmark]
    public void SortedMultiMap_AddRange()
    {
        var map = new SortedMultiMap<string, int>();
        var values = Enumerable.Range(0, Consts.ValuesPerKey).ToArray();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            map.AddRange($"{Consts.KeyPrefix}{k}", values);
        }
    }

    // --- Get benchmarks ---
    [Benchmark]
    public int MultiMapSet_Get()
    {
        int sum = 0;

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            foreach (var v in _setMap.Get($"{Consts.KeyPrefix}{k}"))
            {
                sum += v;
            }
        }

        return sum;
    }

    [Benchmark]
    public int MultiMapList_Get()
    {
        int sum = 0;

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            foreach (var v in _listMap.Get($"{Consts.KeyPrefix}{k}"))
            {
                sum += v;
            }
        }

        return sum;
    }

    [Benchmark]
    public int ConcurrentMultiMap_Get()
    {
        int sum = 0;

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            foreach (var v in _concurrentMap.Get($"{Consts.KeyPrefix}{k}"))
            {
                sum += v;
            }
        }

        return sum;
    }

    [Benchmark]
    public int SortedMultiMap_Get()
    {
        int sum = 0;

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            foreach (var v in _sortedMap.Get($"{Consts.KeyPrefix}{k}"))
            {
                sum += v;
            }
        }

        return sum;
    }

    // --- GetOrDefault benchmarks ---
    [Benchmark]
    public int MultiMapSet_GetOrDefault()
    {
        int sum = 0;

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            foreach (var v in _setMap.GetOrDefault($"{Consts.KeyPrefix}{k}"))
            {
                sum += v;
            }
        }

        return sum;
    }

    [Benchmark]
    public int MultiMapList_GetOrDefault()
    {
        int sum = 0;

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            foreach (var v in _listMap.GetOrDefault($"{Consts.KeyPrefix}{k}"))
            {
                sum += v;
            }
        }

        return sum;
    }

    [Benchmark]
    public int ConcurrentMultiMap_GetOrDefault()
    {
        int sum = 0;

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            foreach (var v in _concurrentMap.GetOrDefault($"{Consts.KeyPrefix}{k}"))
            {
                sum += v;
            }
        }

        return sum;
    }

    [Benchmark]
    public int SortedMultiMap_GetOrDefault()
    {
        int sum = 0;

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            foreach (var v in _sortedMap.GetOrDefault($"{Consts.KeyPrefix}{k}"))
            {
                sum += v;
            }
        }

        return sum;
    }

    // --- Remove benchmarks ---
    [Benchmark]
    public void MultiMapSet_Remove()
    {
        var map = new MultiMapSet<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            string key = $"{Consts.KeyPrefix}{k}";

            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.Add(key, v);
            }
        }

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            string key = $"{Consts.KeyPrefix}{k}";

            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.Remove(key, v);
            }
        }
    }

    [Benchmark]
    public void MultiMapList_Remove()
    {
        var map = new MultiMapList<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            string key = $"{Consts.KeyPrefix}{k}";

            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.Add(key, v);
            }
        }

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            string key = $"{Consts.KeyPrefix}{k}";

            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.Remove(key, v);
            }
        }
    }

    [Benchmark]
    public void ConcurrentMultiMap_Remove()
    {
        var map = new ConcurrentMultiMap<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            string key = $"{Consts.KeyPrefix}{k}";

            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.Add(key, v);
            }
        }

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            string key = $"{Consts.KeyPrefix}{k}";

            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.Remove(key, v);
            }
        }
    }

    [Benchmark]
    public void SortedMultiMap_Remove()
    {
        var map = new SortedMultiMap<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            string key = $"{Consts.KeyPrefix}{k}";

            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.Add(key, v);
            }
        }

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            string key = $"{Consts.KeyPrefix}{k}";

            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.Remove(key, v);
            }
        }
    }

    // --- Clear benchmarks ---
    [Benchmark]
    public void MultiMapSet_Clear()
    {
        var map = new MultiMapSet<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.Add($"{Consts.KeyPrefix}{k}", v);
            }
        }

        map.Clear();
    }

    [Benchmark]
    public void MultiMapList_Clear()
    {
        var map = new MultiMapList<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.Add($"{Consts.KeyPrefix}{k}", v);
            }
        }

        map.Clear();
    }

    [Benchmark]
    public void ConcurrentMultiMap_Clear()
    {
        var map = new ConcurrentMultiMap<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.Add($"{Consts.KeyPrefix}{k}", v);
            }
        }

        map.Clear();
    }

    [Benchmark]
    public void SortedMultiMap_Clear()
    {
        var map = new SortedMultiMap<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.Add($"{Consts.KeyPrefix}{k}", v);
            }
        }

        map.Clear();
    }

    // --- Contains benchmarks ---
    [Benchmark]
    public bool MultiMapSet_Contains()
    {
        var map = new MultiMapSet<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.Contains(Consts.Key50Prefix, Consts.KeyOffset);
    }

    [Benchmark]
    public bool MultiMapList_Contains()
    {
        var map = new MultiMapList<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.Contains(Consts.Key50Prefix, Consts.KeyOffset);
    }

    [Benchmark]
    public bool ConcurrentMultiMap_Contains()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.Contains(Consts.Key50Prefix, Consts.KeyOffset);
    }

    [Benchmark]
    public bool SortedMultiMap_Contains()
    {
        var map = new SortedMultiMap<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.Contains(Consts.Key50Prefix, Consts.KeyOffset);
    }

    // --- ContainsKey benchmarks ---
    [Benchmark]
    public bool MultiMapSet_ContainsKey()
    {
        var map = new MultiMapSet<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.ContainsKey(Consts.Key50Prefix);
    }

    [Benchmark]
    public bool MultiMapList_ContainsKey()
    {
        var map = new MultiMapList<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.ContainsKey(Consts.Key50Prefix);
    }

    [Benchmark]
    public bool ConcurrentMultiMap_ContainsKey()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.ContainsKey(Consts.Key50Prefix);
    }

    [Benchmark]
    public bool SortedMultiMap_ContainsKey()
    {
        var map = new SortedMultiMap<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.ContainsKey(Consts.Key50Prefix);
    }

    // --- GetKeys benchmarks ---
    [Benchmark]
    public int MultiMapSet_GetKeys()
    {
        var map = new MultiMapSet<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.Keys.Count();
    }

    [Benchmark]
    public int MultiMapList_GetKeys()
    {
        var map = new MultiMapList<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.Keys.Count();
    }

    [Benchmark]
    public int ConcurrentMultiMap_GetKeys()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.Keys.Count();
    }

    [Benchmark]
    public int SortedMultiMap_GetKeys()
    {
        var map = new SortedMultiMap<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.Keys.Count();
    }

    // --- TryGetValue benchmarks ---
    [Benchmark]
    public bool MultiMapSet_ContainsKey_Get()
    {
        var map = new MultiMapSet<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.ContainsKey(Consts.Key50Prefix) && map.GetOrDefault(Consts.Key50Prefix).Contains(Consts.KeyOffset);
    }

    [Benchmark]
    public bool MultiMapList_ContainsKey_Get()
    {
        var map = new MultiMapList<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);
        return map.ContainsKey(Consts.Key50Prefix) && map.GetOrDefault(Consts.Key50Prefix).Contains(Consts.KeyOffset);
    }

    [Benchmark]
    public bool ConcurrentMultiMap_ContainsKey_Get()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.ContainsKey(Consts.Key50Prefix) && map.GetOrDefault(Consts.Key50Prefix).Contains(Consts.KeyOffset);
    }

    [Benchmark]
    public bool SortedMultiMap_ContainsKey_Get()
    {
        var map = new SortedMultiMap<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.ContainsKey(Consts.Key50Prefix) && map.GetOrDefault(Consts.Key50Prefix).Contains(Consts.KeyOffset);
    }

    // --- TryGet benchmarks ---
    [Benchmark]
    public bool MultiMapSet_TryGet()
    {
        var map = new MultiMapSet<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.TryGet(Consts.Key50Prefix, out var values) && values.Contains(Consts.KeyOffset);
    }

    [Benchmark]
    public bool MultiMapList_TryGet()
    {
        var map = new MultiMapList<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.TryGet(Consts.Key50Prefix, out var values) && values.Contains(Consts.KeyOffset);
    }

    [Benchmark]
    public bool ConcurrentMultiMap_TryGet()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.TryGet(Consts.Key50Prefix, out var values) && values.Contains(Consts.KeyOffset);
    }

    [Benchmark]
    public bool SortedMultiMap_TryGet()
    {
        var map = new SortedMultiMap<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.TryGet(Consts.Key50Prefix, out var values) && values.Contains(Consts.KeyOffset);
    }

    [Benchmark]
    public bool MultiMapLock_TryGet()
    {
        var map = new MultiMapLock<string, int>();
        map.Add(Consts.Key50Prefix, Consts.KeyOffset);

        return map.TryGet(Consts.Key50Prefix, out var values) && values.Contains(Consts.KeyOffset);
    }

    // --- Keys enumeration benchmarks ---
    [Benchmark]
    public int MultiMapSet_Keys_Enumeration()
    {
        var map = new MultiMapSet<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            map.Add($"{Consts.KeyPrefix}{k}", 0);
        }

        int sum = 0;

        foreach (var key in map.Keys)
        {
            sum += key.Length;
        }

        return sum;
    }

    [Benchmark]
    public int MultiMapList_Keys_Enumeration()
    {
        var map = new MultiMapList<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            map.Add($"{Consts.KeyPrefix}{k}", 0);
        }

        int sum = 0;

        foreach (var key in map.Keys)
        {
            sum += key.Length;
        }

        return sum;
    }

    [Benchmark]
    public int ConcurrentMultiMap_Keys_Enumeration()
    {
        var map = new ConcurrentMultiMap<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            map.Add($"{Consts.KeyPrefix}{k}", 0);
        }

        int sum = 0;

        foreach (var key in map.Keys)
        {
            sum += key.Length;
        }

        return sum;
    }

    [Benchmark]
    public int SortedMultiMap_Keys_Enumeration()
    {
        var map = new SortedMultiMap<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            map.Add($"{Consts.KeyPrefix}{k}", 0);
        }

        int sum = 0;

        foreach (var key in map.Keys)
        {
            sum += key.Length;
        }

        return sum;
    }

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
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
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
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
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
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
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
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
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
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
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
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
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
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
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
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
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
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
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
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
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
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
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
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
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
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
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
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
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
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
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
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.SymmetricExceptWith(other);
    }

    // --- MultiMapList microbenchmarks ---
    [Benchmark]
    public int MultiMapList_Count_AfterAdd()
    {
        var map = new MultiMapList<string, int>();
        map.Add(Consts.Key1Prefix, 1);

        return map.Count;
    }

    [Benchmark]
    public int MultiMapList_Count_AfterRemove()
    {
        var map = new MultiMapList<string, int>();
        map.Add(Consts.Key1Prefix, 1);
        map.Remove(Consts.Key1Prefix, 1);

        return map.Count;
    }

    [Benchmark]
    public void MultiMapList_RemoveKey()
    {
        var map = new MultiMapList<string, int>();
        map.Add(Consts.Key1Prefix, 1);
        map.RemoveKey(Consts.Key1Prefix);
    }

    [Benchmark]
    public void MultiMapList_Clear_Empty()
    {
        var map = new MultiMapList<string, int>();
        map.Clear();
    }

    [Benchmark]
    public bool MultiMapList_ContainsKey_Missing()
    {
        var map = new MultiMapList<string, int>();

        return map.ContainsKey(Consts.KeyMissingPrefix);
    }

    [Benchmark]
    public bool MultiMapList_Remove_Missing()
    {
        var map = new MultiMapList<string, int>();

        return map.Remove(Consts.KeyMissingPrefix, 1);
    }

    [Benchmark]
    public bool MultiMapList_Add_Duplicate()
    {
        var map = new MultiMapList<string, int>();
        map.Add(Consts.Key1Prefix, 1);

        return map.Add(Consts.Key1Prefix, 1);
    }

    // --- MultiMapSet microbenchmarks ---
    [Benchmark]
    public int MultiMapSet_Count_AfterAdd()
    {
        var map = new MultiMapSet<string, int>();
        map.Add(Consts.Key1Prefix, 1);

        return map.Count;
    }

    [Benchmark]
    public int MultiMapSet_Count_AfterRemove()
    {
        var map = new MultiMapSet<string, int>();
        map.Add(Consts.Key1Prefix, 1);
        map.Remove(Consts.Key1Prefix, 1);

        return map.Count;
    }

    [Benchmark]
    public void MultiMapSet_RemoveKey()
    {
        var map = new MultiMapSet<string, int>();
        map.Add(Consts.Key1Prefix, 1);
        map.RemoveKey(Consts.Key1Prefix);
    }

    [Benchmark]
    public void MultiMapSet_Clear_Empty()
    {
        var map = new MultiMapSet<string, int>();
        map.Clear();
    }

    [Benchmark]
    public bool MultiMapSet_ContainsKey_Missing()
    {
        var map = new MultiMapSet<string, int>();

        return map.ContainsKey(Consts.KeyMissingPrefix);
    }

    [Benchmark]
    public bool MultiMapSet_Remove_Missing()
    {
        var map = new MultiMapSet<string, int>();

        return map.Remove(Consts.KeyMissingPrefix, 1);
    }

    [Benchmark]
    public bool MultiMapSet_Add_Duplicate()
    {
        var map = new MultiMapSet<string, int>();
        map.Add(Consts.Key1Prefix, 1);

        return map.Add(Consts.Key1Prefix, 1);
    }

    // --- ConcurrentMultiMap microbenchmarks ---
    [Benchmark]
    public int ConcurrentMultiMap_Count_AfterAdd()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add(Consts.Key1Prefix, 1);

        return map.Count;
    }

    [Benchmark]
    public int ConcurrentMultiMap_Count_AfterRemove()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add(Consts.Key1Prefix, 1);
        map.Remove(Consts.Key1Prefix, 1);

        return map.Count;
    }

    [Benchmark]
    public void ConcurrentMultiMap_RemoveKey()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add(Consts.Key1Prefix, 1);
        map.RemoveKey(Consts.Key1Prefix);
    }

    [Benchmark]
    public void ConcurrentMultiMap_Clear_Empty()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Clear();
    }

    [Benchmark]
    public bool ConcurrentMultiMap_ContainsKey_Missing()
    {
        var map = new ConcurrentMultiMap<string, int>();

        return map.ContainsKey(Consts.KeyMissingPrefix);
    }

    [Benchmark]
    public bool ConcurrentMultiMap_Remove_Missing()
    {
        var map = new ConcurrentMultiMap<string, int>();

        return map.Remove(Consts.KeyMissingPrefix, 1);
    }

    [Benchmark]
    public bool ConcurrentMultiMap_Add_Duplicate()
    {
        var map = new ConcurrentMultiMap<string, int>();
        map.Add(Consts.Key1Prefix, 1);

        return map.Add(Consts.Key1Prefix, 1);
    }

    // --- SortedMultiMap microbenchmarks ---
    [Benchmark]
    public int SortedMultiMap_Count_AfterAdd()
    {
        var map = new SortedMultiMap<string, int>();
        map.Add(Consts.Key1Prefix, 1);

        return map.Count;
    }

    [Benchmark]
    public int SortedMultiMap_Count_AfterRemove()
    {
        var map = new SortedMultiMap<string, int>();
        map.Add(Consts.Key1Prefix, 1);
        map.Remove(Consts.Key1Prefix, 1);

        return map.Count;
    }

    [Benchmark]
    public void SortedMultiMap_RemoveKey()
    {
        var map = new SortedMultiMap<string, int>();
        map.Add(Consts.Key1Prefix, 1);
        map.RemoveKey(Consts.Key1Prefix);
    }

    [Benchmark]
    public void SortedMultiMap_Clear_Empty()
    {
        var map = new SortedMultiMap<string, int>();
        map.Clear();
    }

    [Benchmark]
    public bool SortedMultiMap_ContainsKey_Missing()
    {
        var map = new SortedMultiMap<string, int>();

        return map.ContainsKey(Consts.KeyMissingPrefix);
    }

    [Benchmark]
    public bool SortedMultiMap_Remove_Missing()
    {
        var map = new SortedMultiMap<string, int>();

        return map.Remove(Consts.KeyMissingPrefix, 1);
    }

    [Benchmark]
    public bool SortedMultiMap_Add_Duplicate()
    {
        var map = new SortedMultiMap<string, int>();
        map.Add(Consts.Key1Prefix, 1);

        return map.Add(Consts.Key1Prefix, 1);
    }

    // ────────────────────────────────────────────────────────────────────
    // Benchmarks for newly added interface members
    // ────────────────────────────────────────────────────────────────────

    #region KeyCount Property Benchmarks

    [Benchmark]
    public int MultiMapSet_KeyCount() => _setMap.KeyCount;

    [Benchmark]
    public int MultiMapList_KeyCount() => _listMap.KeyCount;

    [Benchmark]
    public int ConcurrentMultiMap_KeyCount() => _concurrentMap.KeyCount;

    [Benchmark]
    public int SortedMultiMap_KeyCount() => _sortedMap.KeyCount;

    #endregion

    #region Values Property Benchmarks

    [Benchmark]
    public int MultiMapSet_Values()
    {
        int count = 0;
        foreach (var value in _setMap.Values)
            count++;
        return count;
    }

    [Benchmark]
    public int MultiMapList_Values()
    {
        int count = 0;
        foreach (var value in _listMap.Values)
            count++;
        return count;
    }

    [Benchmark]
    public int ConcurrentMultiMap_Values()
    {
        int count = 0;
        foreach (var value in _concurrentMap.Values)
            count++;
        return count;
    }

    [Benchmark]
    public int SortedMultiMap_Values()
    {
        int count = 0;
        foreach (var value in _sortedMap.Values)
            count++;
        return count;
    }

    #endregion

    #region GetValuesCount Method Benchmarks

    [Benchmark]
    public int MultiMapSet_GetValuesCount() => _setMap.GetValuesCount($"{Consts.KeyPrefix}0");

    [Benchmark]
    public int MultiMapList_GetValuesCount() => _listMap.GetValuesCount($"{Consts.KeyPrefix}0");

    [Benchmark]
    public int ConcurrentMultiMap_GetValuesCount() => _concurrentMap.GetValuesCount($"{Consts.KeyPrefix}0");

    [Benchmark]
    public int SortedMultiMap_GetValuesCount() => _sortedMap.GetValuesCount($"{Consts.KeyPrefix}0");

    #endregion

    #region Indexer Benchmarks

    [Benchmark]
    public int MultiMapSet_Indexer() => _setMap[$"{Consts.KeyPrefix}0"].Count();

    [Benchmark]
    public int MultiMapList_Indexer() => _listMap[$"{Consts.KeyPrefix}0"].Count();

    [Benchmark]
    public int ConcurrentMultiMap_Indexer() => _concurrentMap[$"{Consts.KeyPrefix}0"].Count();

    [Benchmark]
    public int SortedMultiMap_Indexer() => _sortedMap[$"{Consts.KeyPrefix}0"].Count();

    #endregion

    #region AddRange KeyValuePair Benchmarks

    [Benchmark]
    public void MultiMapSet_AddRange_KeyValuePair()
    {
        var map = new MultiMapSet<string, int>();
        var pairs = Enumerable.Range(0, Consts.KeyCount)
            .SelectMany(k => Enumerable.Range(0, Consts.ValuesPerKey)
                .Select(v => new KeyValuePair<string, int>($"{Consts.KeyPrefix}{k}", v)));

        map.AddRange(pairs);
    }

    [Benchmark]
    public void MultiMapList_AddRange_KeyValuePair()
    {
        var map = new MultiMapList<string, int>();
        var pairs = Enumerable.Range(0, Consts.KeyCount)
            .SelectMany(k => Enumerable.Range(0, Consts.ValuesPerKey)
                .Select(v => new KeyValuePair<string, int>($"{Consts.KeyPrefix}{k}", v)));

        map.AddRange(pairs);
    }

    [Benchmark]
    public void ConcurrentMultiMap_AddRange_KeyValuePair()
    {
        var map = new ConcurrentMultiMap<string, int>();
        var pairs = Enumerable.Range(0, Consts.KeyCount)
            .SelectMany(k => Enumerable.Range(0, Consts.ValuesPerKey)
                .Select(v => new KeyValuePair<string, int>($"{Consts.KeyPrefix}{k}", v)));

        map.AddRange(pairs);
    }

    [Benchmark]
    public void SortedMultiMap_AddRange_KeyValuePair()
    {
        var map = new SortedMultiMap<string, int>();
        var pairs = Enumerable.Range(0, Consts.KeyCount)
            .SelectMany(k => Enumerable.Range(0, Consts.ValuesPerKey)
                .Select(v => new KeyValuePair<string, int>($"{Consts.KeyPrefix}{k}", v)));

        map.AddRange(pairs);
    }

    #endregion

    #region RemoveRange Benchmarks

    [Benchmark]
    public int MultiMapSet_RemoveRange()
    {
        var map = new MultiMapSet<string, int>();
        for (int k = 0; k < Consts.KeyCount; k++)
            for (int v = 0; v < Consts.ValuesPerKey; v++)
                map.Add($"{Consts.KeyPrefix}{k}", v);

        var pairs = Enumerable.Range(0, Consts.KeyCount / 2)
            .SelectMany(k => Enumerable.Range(0, Consts.ValuesPerKey)
                .Select(v => new KeyValuePair<string, int>($"{Consts.KeyPrefix}{k}", v)));

        return map.RemoveRange(pairs);
    }

    [Benchmark]
    public int MultiMapList_RemoveRange()
    {
        var map = new MultiMapList<string, int>();
        for (int k = 0; k < Consts.KeyCount; k++)
            for (int v = 0; v < Consts.ValuesPerKey; v++)
                map.Add($"{Consts.KeyPrefix}{k}", v);

        var pairs = Enumerable.Range(0, Consts.KeyCount / 2)
            .SelectMany(k => Enumerable.Range(0, Consts.ValuesPerKey)
                .Select(v => new KeyValuePair<string, int>($"{Consts.KeyPrefix}{k}", v)));

        return map.RemoveRange(pairs);
    }

    [Benchmark]
    public int ConcurrentMultiMap_RemoveRange()
    {
        var map = new ConcurrentMultiMap<string, int>();
        for (int k = 0; k < Consts.KeyCount; k++)
            for (int v = 0; v < Consts.ValuesPerKey; v++)
                map.Add($"{Consts.KeyPrefix}{k}", v);

        var pairs = Enumerable.Range(0, Consts.KeyCount / 2)
            .SelectMany(k => Enumerable.Range(0, Consts.ValuesPerKey)
                .Select(v => new KeyValuePair<string, int>($"{Consts.KeyPrefix}{k}", v)));

        return map.RemoveRange(pairs);
    }

    [Benchmark]
    public int SortedMultiMap_RemoveRange()
    {
        var map = new SortedMultiMap<string, int>();
        for (int k = 0; k < Consts.KeyCount; k++)
            for (int v = 0; v < Consts.ValuesPerKey; v++)
                map.Add($"{Consts.KeyPrefix}{k}", v);

        var pairs = Enumerable.Range(0, Consts.KeyCount / 2)
            .SelectMany(k => Enumerable.Range(0, Consts.ValuesPerKey)
                .Select(v => new KeyValuePair<string, int>($"{Consts.KeyPrefix}{k}", v)));

        return map.RemoveRange(pairs);
    }

    #endregion

    #region RemoveWhere Benchmarks

    [Benchmark]
    public int MultiMapSet_RemoveWhere()
    {
        var map = new MultiMapSet<string, int>();
        for (int v = 0; v < Consts.ValuesPerKey * 2; v++)
            map.Add(Consts.Key1Prefix, v);

        return map.RemoveWhere(Consts.Key1Prefix, v => v % 2 == 0);
    }

    [Benchmark]
    public int MultiMapList_RemoveWhere()
    {
        var map = new MultiMapList<string, int>();
        for (int v = 0; v < Consts.ValuesPerKey * 2; v++)
            map.Add(Consts.Key1Prefix, v);

        return map.RemoveWhere(Consts.Key1Prefix, v => v % 2 == 0);
    }

    [Benchmark]
    public int ConcurrentMultiMap_RemoveWhere()
    {
        var map = new ConcurrentMultiMap<string, int>();
        for (int v = 0; v < Consts.ValuesPerKey * 2; v++)
            map.Add(Consts.Key1Prefix, v);

        return map.RemoveWhere(Consts.Key1Prefix, v => v % 2 == 0);
    }

    [Benchmark]
    public int SortedMultiMap_RemoveWhere()
    {
        var map = new SortedMultiMap<string, int>();
        for (int v = 0; v < Consts.ValuesPerKey * 2; v++)
            map.Add(Consts.Key1Prefix, v);

        return map.RemoveWhere(Consts.Key1Prefix, v => v % 2 == 0);
    }

    #endregion
}