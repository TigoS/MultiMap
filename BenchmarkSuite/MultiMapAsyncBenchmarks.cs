using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;
using MultiMap.Entities;
using System.Collections.Generic;
using System.Linq;

namespace BenchmarkSuite;

[CPUUsageDiagnoser]
public class MultiMapAsyncBenchmarks
{
    private MultiMapAsync<string, int> _map = null!;
    private string[] _keys = null!;

    [GlobalSetup]
    public void Setup()
    {
        _map = new MultiMapAsync<string, int>();
        _keys = new string[Consts.KeyCount];

        for (int i = 0; i < Consts.KeyCount; i++)
        {
            _keys[i] = $"{Consts.KeyPrefix}{i}";

            for (int j = 0; j < Consts.ValuesPerKey; j++)
            {
                _map.AddAsync(_keys[i], j).GetAwaiter().GetResult();
            }
        }
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _map.DisposeAsync().GetAwaiter().GetResult();
    }

    [Benchmark]
    public void MultiMapAsync_Add()
    {
        var map = new MultiMapAsync<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            string key = $"{Consts.KeyPrefix}{k}";

            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.AddAsync(key, v).GetAwaiter().GetResult();
            }
        }

        map.DisposeAsync().GetAwaiter().GetResult();
    }

    [Benchmark]
    public void MultiMapAsync_AddRange()
    {
        var map = new MultiMapAsync<string, int>();
        var values = Enumerable.Range(0, Consts.ValuesPerKey).ToArray();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            map.AddRangeAsync($"{Consts.KeyPrefix}{k}", values).GetAwaiter().GetResult();
        }

        map.DisposeAsync().GetAwaiter().GetResult();
    }

    [Benchmark]
    public int MultiMapAsync_Get()
    {
        int sum = 0;

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            foreach (var v in _map.GetAsync(_keys[k]).GetAwaiter().GetResult())
            {
                sum += v;
            }
        }

        return sum;
    }

    [Benchmark]
    public int MultiMapAsync_GetOrDefault()
    {
        int sum = 0;

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            foreach (var v in _map.GetOrDefaultAsync(_keys[k]).GetAwaiter().GetResult())
            {
                sum += v;
            }
        }

        return sum;
    }

    [Benchmark]
    public void MultiMapAsync_Remove()
    {
        var map = new MultiMapAsync<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            string key = $"{Consts.KeyPrefix}{k}";

            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.AddAsync(key, v).GetAwaiter().GetResult();
            }
        }

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            string key = $"{Consts.KeyPrefix}{k}";

            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.RemoveAsync(key, v).GetAwaiter().GetResult();
            }
        }
    }

    [Benchmark]
    public void MultiMapAsync_Clear()
    {
        var map = new MultiMapAsync<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.AddAsync($"{Consts.KeyPrefix}{k}", v).GetAwaiter().GetResult();
            }
        }

        map.ClearAsync().GetAwaiter().GetResult();
    }

    [Benchmark]
    public bool MultiMapAsync_Contains()
    {
        return _map.ContainsAsync(Consts.Key50Prefix, Consts.KeyOffset).GetAwaiter().GetResult();
    }

    [Benchmark]
    public bool MultiMapAsync_ContainsKey()
    {
        return _map.ContainsKeyAsync(Consts.Key50Prefix).GetAwaiter().GetResult();
    }

    [Benchmark]
    public int MultiMapAsync_GetCount()
    {
        return _map.GetCountAsync().GetAwaiter().GetResult();
    }

    [Benchmark]
    public int MultiMapAsync_GetKeys()
    {
        return _map.GetKeysAsync().GetAwaiter().GetResult().Count();
    }

    [Benchmark]
    public bool MultiMapAsync_TryGet()
    {
        var (found, values) = _map.TryGetAsync(Consts.Key50Prefix).GetAwaiter().GetResult();
        return found && values.Contains(Consts.KeyOffset);
    }

    [Benchmark]
    public void MultiMapAsync_Union()
    {
        var target = new MultiMapAsync<string, int>();
        var other = new MultiMapAsync<string, int>();

        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.AddAsync($"{Consts.KeyPrefix}{k}", v).GetAwaiter().GetResult();
                other.AddAsync($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset).GetAwaiter().GetResult();
            }
        }

        target.UnionAsync(other).GetAwaiter().GetResult();
    }

    [Benchmark]
    public void MultiMapAsync_Intersect()
    {
        var target = new MultiMapAsync<string, int>();
        var other = new MultiMapAsync<string, int>();

        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.AddAsync($"{Consts.KeyPrefix}{k}", v).GetAwaiter().GetResult();
                other.AddAsync($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset).GetAwaiter().GetResult();
            }
        }

        target.IntersectAsync(other).GetAwaiter().GetResult();
    }

    [Benchmark]
    public void MultiMapAsync_ExceptWith()
    {
        var target = new MultiMapAsync<string, int>();
        var other = new MultiMapAsync<string, int>();

        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.AddAsync($"{Consts.KeyPrefix}{k}", v).GetAwaiter().GetResult();
                other.AddAsync($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset).GetAwaiter().GetResult();
            }
        }

        target.ExceptWithAsync(other).GetAwaiter().GetResult();
    }

    [Benchmark]
    public void MultiMapAsync_SymmetricExceptWith()
    {
        var target = new MultiMapAsync<string, int>();
        var other = new MultiMapAsync<string, int>();

        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.AddAsync($"{Consts.KeyPrefix}{k}", v).GetAwaiter().GetResult();
                other.AddAsync($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset).GetAwaiter().GetResult();
            }
        }

        target.SymmetricExceptWithAsync(other).GetAwaiter().GetResult();
    }

    // ────────────────────────────────────────────────────────────────────
    // Benchmarks for newly added interface members
    // ────────────────────────────────────────────────────────────────────

    #region New Property/Method Benchmarks

    [Benchmark]
    public int MultiMapAsync_GetKeyCount()
    {
        return _map.GetKeyCountAsync().GetAwaiter().GetResult();
    }

    [Benchmark]
    public int MultiMapAsync_GetValues()
    {
        int count = 0;
        var values = _map.GetValuesAsync().GetAwaiter().GetResult();
        foreach (var value in values)
            count++;
        return count;
    }

    [Benchmark]
    public int MultiMapAsync_GetValuesCount()
    {
        return _map.GetValuesCountAsync(Consts.Key50Prefix).GetAwaiter().GetResult();
    }

    [Benchmark]
    public void MultiMapAsync_AddRange_KeyValuePair()
    {
        var map = new MultiMapAsync<string, int>();
        var pairs = Enumerable.Range(0, Consts.KeyCount)
            .SelectMany(k => Enumerable.Range(0, Consts.ValuesPerKey)
                .Select(v => new KeyValuePair<string, int>($"{Consts.KeyPrefix}{k}", v)));

        map.AddRangeAsync(pairs).GetAwaiter().GetResult();
    }

    [Benchmark]
    public int MultiMapAsync_RemoveRange()
    {
        var map = new MultiMapAsync<string, int>();
        for (int k = 0; k < Consts.KeyCount; k++)
            for (int v = 0; v < Consts.ValuesPerKey; v++)
                map.AddAsync($"{Consts.KeyPrefix}{k}", v).GetAwaiter().GetResult();

        var pairs = Enumerable.Range(0, Consts.KeyCount / 2)
            .SelectMany(k => Enumerable.Range(0, Consts.ValuesPerKey)
                .Select(v => new KeyValuePair<string, int>($"{Consts.KeyPrefix}{k}", v)));

        return map.RemoveRangeAsync(pairs).GetAwaiter().GetResult();
    }

    [Benchmark]
    public int MultiMapAsync_RemoveWhere()
    {
        var map = new MultiMapAsync<string, int>();
        for (int v = 0; v < Consts.ValuesPerKey * 2; v++)
            map.AddAsync(Consts.Key1Prefix, v).GetAwaiter().GetResult();

        return map.RemoveWhereAsync(Consts.Key1Prefix, v => v % 2 == 0).GetAwaiter().GetResult();
    }

    #endregion
}