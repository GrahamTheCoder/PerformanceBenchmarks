using System.Collections.Frozen;
using System.Collections.Immutable;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace DotNet8Benchmarks.FrozenDictionary;

public class FrozenDictionary
{

    [Params(2000, 10000)] public int UpperLimit;
    [Params(1000)] public int RequestCount;
    [Params(300, 600)] public int EntryCount;
    private Random _random;
    private IDictionary<int, string> _dictionary;
    private IDictionary<int, string> _unoptimisedFrozenDictionary;
    private IDictionary<int, string> _optimisedFrozenDictionary;
    private ImmutableArray<string?> _immutableArray;


    [GlobalSetup]
    public void GlobalSetup()
    {
        _random = new Random();

        var randomKeyValuePairs = GetRandomKeyValuePairs(EntryCount)
            .GroupBy(x => x.Key).Select(g => g.First()).ToArray();

        _dictionary = randomKeyValuePairs.ToDictionary(x => x.Key, x => x.Value);
        _unoptimisedFrozenDictionary = randomKeyValuePairs.ToFrozenDictionary(false);
        _optimisedFrozenDictionary = randomKeyValuePairs.ToFrozenDictionary(true);

        var sparseArray = new string?[UpperLimit + 1];
        foreach (var kvp in randomKeyValuePairs)
        {
            sparseArray[kvp.Key] = kvp.Value;
        }
        _immutableArray = sparseArray.ToImmutableArray();
    }

    [Benchmark] public string? Dictionary() => LookupItems(_dictionary);
    [Benchmark] public string? UnoptimisedFrozenDictionary() => LookupItems(_unoptimisedFrozenDictionary);
    [Benchmark] public string? OptimisedFrozenDictionary() => LookupItems(_optimisedFrozenDictionary);
    [Benchmark(Baseline = true)] public string? ImmutableArraySpan() => LookupItems(_immutableArray.AsSpan());

    private IEnumerable<KeyValuePair<int, string>> GetRandomKeyValuePairs(int count)
    {
        for (var i = 0; i < count; i++)
            yield return new KeyValuePair<int, string>(_random.Next(UpperLimit), _random.Next().ToString());
    }

    private string? LookupItems(IDictionary<int, string> dictionary)
    {
        var res = "";
        for (var i = 0; i < RequestCount; i++)
        {
            var index = _random.Next(UpperLimit);
            res = dictionary.TryGetValue(index, out var str) ? str : null;
        }

        return res;
    }

    private string? LookupItems(ReadOnlySpan<string?> readOnlySpan)
    {
        var res = "";
        for (var i = 0; i < RequestCount; i++)
        {
            var index = _random.Next(UpperLimit);
            res = readOnlySpan[index];
        }

        return res;
    }
}