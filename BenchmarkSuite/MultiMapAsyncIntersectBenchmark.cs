using BenchmarkDotNet.Attributes;
using MultiMap.Entities;
using MultiMap.Helpers;
using Microsoft.VSDiagnostics;

namespace BenchmarkSuite;
[CPUUsageDiagnoser]
public class MultiMapAsyncIntersectBenchmark
{
    private const int KeyCount = 50;
    private const int ValuesPerKey = 20;
    private const int KeyOffset = 25;
    private const int ValueOffset = 10;
    [Benchmark]
    public void MultiMapAsyncHelper_Intersect()
    {
        var target = new MultiMapAsync<string, int>();
        var other = new MultiMapAsync<string, int>();
        for (int k = 0; k < KeyCount; k++)
        {
            for (int v = 0; v < ValuesPerKey; v++)
            {
                target.AddAsync($"key{k}", v).GetAwaiter().GetResult();
                other.AddAsync($"key{k + KeyOffset}", v + ValueOffset).GetAwaiter().GetResult();
            }
        }

        target.IntersectAsync(other).GetAwaiter().GetResult();
    }
}