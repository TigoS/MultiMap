using System.Linq;
using BenchmarkDotNet.Attributes;
using MultiMap.Entities;
using Microsoft.VSDiagnostics;

namespace BenchmarkSuite;

[CPUUsageDiagnoser]
public class MultiMapLockBenchmarks
{
    private MultiMapLock<string, int> _map = null!;
    private string[] _keys = null!;

    [GlobalSetup]
    public void Setup()
    {
        _map = new MultiMapLock<string, int>();
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

    [GlobalCleanup]
    public void Cleanup()
    {
        _map.Dispose();
    }

    [Benchmark]
    public void MultiMapLock_Add()
    {
        var map = new MultiMapLock<string, int>();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            string key = $"{Consts.KeyPrefix}{k}";

            for (int v = 0; v < Consts.ValuesPerKey; v++)
            {
                map.Add(key, v);
            }
        }

        map.Dispose();
    }

    [Benchmark]
    public void MultiMapLock_AddRange()
    {
        var map = new MultiMapLock<string, int>();
        var values = Enumerable.Range(0, Consts.ValuesPerKey).ToArray();

        for (int k = 0; k < Consts.KeyCount; k++)
        {
            map.AddRange($"{Consts.KeyPrefix}{k}", values);
        }

        map.Dispose();
    }

    [Benchmark]
    public int MultiMapLock_Get()
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

    [Benchmark]
    public int MultiMapLock_GetOrDefault()
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
    public void MultiMapLock_Remove()
    {
        var map = new MultiMapLock<string, int>();

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
    public void MultiMapLock_Clear()
    {
        var map = new MultiMapLock<string, int>();

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
    public bool MultiMapLock_Contains() => _map.Contains(Consts.Key50Prefix, Consts.KeyOffset);

    [Benchmark]
    public bool MultiMapLock_ContainsKey() => _map.ContainsKey(Consts.Key50Prefix);

    [Benchmark]
    public int MultiMapLock_Count() => _map.Count;

    [Benchmark]
    public int MultiMapLock_GetKeys() => _map.Keys.Count();

    [Benchmark]
    public void MultiMapLock_Union()
    {
        var target = new MultiMapLock<string, int>();
        var other = new MultiMapLock<string, int>();

        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.Union(other);
        target.Dispose();
        other.Dispose();
    }

    [Benchmark]
    public void MultiMapLock_Intersect()
    {
        var target = new MultiMapLock<string, int>();
        var other = new MultiMapLock<string, int>();

        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.Intersect(other);
        target.Dispose();
        other.Dispose();
    }

    [Benchmark]
    public void MultiMapLock_ExceptWith()
    {
        var target = new MultiMapLock<string, int>();
        var other = new MultiMapLock<string, int>();

        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.ExceptWith(other);
        target.Dispose();
        other.Dispose();
    }

    [Benchmark]
    public void MultiMapLock_SymmetricExceptWith()
    {
        var target = new MultiMapLock<string, int>();
        var other = new MultiMapLock<string, int>();

        for (int k = 0; k < Consts.SetOpKeyCount; k++)
        {
            for (int v = 0; v < Consts.SetOpValuesPerKey; v++)
            {
                target.Add($"{Consts.KeyPrefix}{k}", v);
                other.Add($"{Consts.KeyPrefix}{k + Consts.KeyOffset}", v + Consts.ValueOffset);
            }
        }

        target.SymmetricExceptWith(other);
        target.Dispose();
        other.Dispose();
    }
}