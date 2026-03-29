using System.Linq;
using BenchmarkDotNet.Attributes;
using MultiMap.Entities;
using MultiMap.Helpers;
using Microsoft.VSDiagnostics;

namespace BenchmarkSuite;
[CPUUsageDiagnoser]
public class MultiMapLockBenchmarks
{
    private const int KeyCount = 100;
    private const int ValuesPerKey = 50;
    private MultiMapLock<string, int> _map = null!;
    private string[] _keys = null!;
    [GlobalSetup]
    public void Setup()
    {
        _map = new MultiMapLock<string, int>();
        _keys = new string[KeyCount];
        for (int i = 0; i < KeyCount; i++)
        {
            _keys[i] = $"key{i}";
            for (int j = 0; j < ValuesPerKey; j++)
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
        for (int k = 0; k < KeyCount; k++)
        {
            string key = $"key{k}";
            for (int v = 0; v < ValuesPerKey; v++)
            {
                map.Add(key, v);
            }
        }

        map.Dispose();
    }

    [Benchmark]
    public int MultiMapLock_Get()
    {
        int sum = 0;
        for (int k = 0; k < KeyCount; k++)
        {
            foreach (var v in _map.Get(_keys[k]))
                sum += v;
        }

        return sum;
    }

    [Benchmark]
    public bool MultiMapLock_Contains() => _map.Contains("key50", 25);
    [Benchmark]
    public bool MultiMapLock_ContainsKey() => _map.ContainsKey("key50");
    [Benchmark]
    public int MultiMapLock_Count() => _map.Count;
    [Benchmark]
    public int MultiMapLock_GetKeys() => _map.Keys.Count();
    [Benchmark]
    public void MultiMapLock_Intersect()
    {
        var target = new MultiMapLock<string, int>();
        var other = new MultiMapLock<string, int>();
        for (int k = 0; k < 50; k++)
        {
            for (int v = 0; v < 20; v++)
            {
                target.Add($"key{k}", v);
                other.Add($"key{k + 25}", v + 10);
            }
        }

        target.Intersect(other);
        target.Dispose();
        other.Dispose();
    }
}