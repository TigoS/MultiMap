using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;
using MultiMap.Entities;

namespace BenchmarkSuite;

[CPUUsageDiagnoser]
public class SimpleMultiMapBenchmarks
{
    private SimpleMultiMap<string, int> _map = null!;
    private string[] _keys = null!;

    [GlobalSetup]
    public void Setup()
    {
        _map = new SimpleMultiMap<string, int>();
        _keys = new string[Consts.KeyCount];

        for (int i = 0; i < Consts.KeyCount; i++)
        {
            _keys[i] = $"{Consts.KeyPrefix}{i}";

            for (int j = 0; j < Consts.ValuesPerKey; j++)
            {
                _map.Add(_keys[i], j);
            }
        }
    }

    // --- Add benchmarks ---

    [Benchmark]
    public void SimpleMultiMap_Add()
    {
        var map = new SimpleMultiMap<string, int>();

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
    public bool SimpleMultiMap_Add_Duplicate()
    {
        var map = new SimpleMultiMap<string, int>();
        map.Add(Consts.Key1Prefix, 1);

        return map.Add(Consts.Key1Prefix, 1);
    }

    // --- Get benchmarks ---

    [Benchmark]
    public int SimpleMultiMap_Get()
    {
        int sum = 0;

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            foreach (var v in _map.Get(_keys[k]))
            {
                sum += v;
            }
        }

        return sum;
    }

    // --- GetOrDefault benchmarks ---

    [Benchmark]
    public int SimpleMultiMap_GetOrDefault()
    {
        int sum = 0;

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            foreach (var v in _map.GetOrDefault(_keys[k]))
            {
                sum += v;
            }
        }

        return sum;
    }

    [Benchmark]
    public int SimpleMultiMap_GetOrDefault_Missing()
    {
        int sum = 0;

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            foreach (var v in _map.GetOrDefault($"{Consts.KeyMissingPrefix}{k}"))
            {
                sum += v;
            }
        }

        return sum;
    }

    // --- Remove benchmarks ---

    [Benchmark]
    public void SimpleMultiMap_Remove()
    {
        var map = new SimpleMultiMap<string, int>();

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

    // --- Clear(key) benchmarks ---

    [Benchmark]
    public void SimpleMultiMap_Clear()
    {
        var map = new SimpleMultiMap<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.Add($"{Consts.KeyPrefix}{k}", v);
            }
        }

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            map.Clear($"{Consts.KeyPrefix}{k}");
        }
    }

    // --- Flatten benchmarks ---

    [Benchmark]
    public int SimpleMultiMap_Flatten()
    {
        int count = 0;

        foreach (var kvp in _map.Flatten())
        {
            count++;
        }

        return count;
    }

    // --- Enumeration benchmarks ---

    [Benchmark]
    public int SimpleMultiMap_Enumerate()
    {
        int count = 0;

        foreach (var kvp in _map)
        {
            count++;
        }

        return count;
    }
}
