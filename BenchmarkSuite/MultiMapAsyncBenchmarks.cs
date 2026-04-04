using System.Linq;
using BenchmarkDotNet.Attributes;
using MultiMap.Entities;
using Microsoft.VSDiagnostics;

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
}