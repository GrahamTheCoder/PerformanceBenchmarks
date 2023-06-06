# FrozenDictionary

When dot net 7 came out, I read through Stephen Toub's entire [performance blog post](https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7/), learned many interesting things, and was very excited to try it out with the Vue data analysis platform I work on at Savanta. There were so many improvements it seemed inevitable it would make something better.

It didn't. So I had a think back through the fixes, and none of them directly applied to the main work that takes CPU cycles in our app: dictionary lookups. There was mention in the blog post that one of their benchmarks for dictionary lookups had improved anyway, but alas, it didn't seem to cover the case our app hits. I was a bit disappointed since it felt like a lot of data heavy apps must be bottlenecked by dictionary lookups, and they certainly aren't fully optimised in the standard class...

Enter dot net 8: `ToFrozenDictionary(true)`. As the name implies, you can't add to a frozen dictionary which is what gives the opportunity for various optimisations - preparing these read-time optimisations can slow down the initial creation, which is why a `true` parameter shown above is required to opt in to these optimisations. For example:
* An int key with the identity hash function doesn't need to store the hash code again, which saves storage space and an extra layer of indirection.
* Multiple different prime numbers of buckets are tested to select the one with the lowest collision rate.

If you're doing testing yourself, it's worth understanding that performance differs for int32, string, known comparable types, and other types, and that there's a limit of 500 elements after which the search for the best hash key is somewhat scaled back.

I'll use indexing into a sparse span as the baseline - 1 unit of time - this is *about* the fastest a lookup could ever be (which takes 5 microseconds on my machine):
* Optimised frozen dictionary lookup: 2.5 units (2.18-3.18)
* Standard/unoptimised dictionary lookup: 5 units (4.4-5.32)

So if you have a dictionary that's unchanged for a large number of reads and those reads are a bottleneck in the application, this is *definitely* a worthwhile trying out, though as always:
* Profile some real use cases that are causing issues
* Benchmark these cases using BenchmarkDotNet
* Get the right algorithm and data structures
* Only then start making micro-optimisations like this

### Full table of results

#### long


|                      Method | UpperLimit | RequestCount | EntryCount |      Mean |     Error |    StdDev |    Median | Ratio |
|---------------------------- |----------- |------------- |----------- |----------:|----------:|----------:|----------:|------:|
|                  Dictionary |       2000 |         1000 |        300 | 21.647 us | 0.3000 us | 0.2505 us | 21.538 us |  4.49 |
| UnoptimisedFrozenDictionary |       2000 |         1000 |        300 | 22.070 us | 0.2291 us | 0.1913 us | 22.066 us |  4.58 |
|   OptimisedFrozenDictionary |       2000 |         1000 |        300 | 12.966 us | 0.2555 us | 0.4339 us | 12.895 us |  2.66 |
|          ImmutableArraySpan |       2000 |         1000 |        300 |  4.820 us | 0.0941 us | 0.1155 us |  4.782 us |  1.00 |
|                             |            |              |            |           |           |           |           |       |
|                  Dictionary |       2000 |         1000 |        600 | 22.806 us | 0.4480 us | 0.6426 us | 22.677 us |  4.72 |
| UnoptimisedFrozenDictionary |       2000 |         1000 |        600 | 22.694 us | 0.4361 us | 0.5023 us | 22.478 us |  4.68 |
|   OptimisedFrozenDictionary |       2000 |         1000 |        600 | 15.423 us | 0.3029 us | 0.3606 us | 15.425 us |  3.18 |
|          ImmutableArraySpan |       2000 |         1000 |        600 |  4.841 us | 0.0966 us | 0.1417 us |  4.817 us |  1.00 |
|                             |            |              |            |           |           |           |           |       |
|                  Dictionary |      10000 |         1000 |        300 | 21.978 us | 0.4360 us | 1.0362 us | 21.634 us |  4.84 |
| UnoptimisedFrozenDictionary |      10000 |         1000 |        300 | 21.927 us | 0.3994 us | 0.3541 us | 21.752 us |  4.82 |
|   OptimisedFrozenDictionary |      10000 |         1000 |        300 | 10.624 us | 0.1988 us | 0.1763 us | 10.567 us |  2.34 |
|          ImmutableArraySpan |      10000 |         1000 |        300 |  4.550 us | 0.0572 us | 0.0507 us |  4.560 us |  1.00 |
|                             |            |              |            |           |           |           |           |       |
|                  Dictionary |      10000 |         1000 |        600 | 21.424 us | 0.4183 us | 0.4980 us | 21.211 us |  4.73 |
| UnoptimisedFrozenDictionary |      10000 |         1000 |        600 | 21.722 us | 0.4319 us | 0.9660 us | 21.503 us |  4.97 |
|   OptimisedFrozenDictionary |      10000 |         1000 |        600 | 11.229 us | 0.2246 us | 0.2841 us | 11.146 us |  2.48 |
|          ImmutableArraySpan |      10000 |         1000 |        600 |  4.535 us | 0.0885 us | 0.1087 us |  4.516 us |  1.00 |

#### int32

|                      Method | UpperLimit | RequestCount | EntryCount |      Mean |     Error |    StdDev |    Median | Ratio |
|---------------------------- |----------- |------------- |----------- |----------:|----------:|----------:|----------:|------:|
|                  Dictionary |       2000 |         1000 |        300 | 22.223 us | 0.4020 us | 0.3949 us | 22.206 us |  4.50 |
| UnoptimisedFrozenDictionary |       2000 |         1000 |        300 | 22.100 us | 0.4331 us | 0.6211 us | 21.922 us |  4.45 |
|   OptimisedFrozenDictionary |       2000 |         1000 |        300 | 12.079 us | 0.2169 us | 0.2029 us | 11.989 us |  2.45 |
|          ImmutableArraySpan |       2000 |         1000 |        300 |  4.931 us | 0.0927 us | 0.2292 us |  4.869 us |  1.00 |
|                             |            |              |            |           |           |           |           |       |
|                  Dictionary |       2000 |         1000 |        600 | 23.662 us | 0.4571 us | 1.2966 us | 23.282 us |  4.94 |
| UnoptimisedFrozenDictionary |       2000 |         1000 |        600 | 23.730 us | 0.4686 us | 0.7432 us | 23.632 us |  5.32 |
|   OptimisedFrozenDictionary |       2000 |         1000 |        600 | 14.628 us | 0.2911 us | 0.2723 us | 14.572 us |  3.22 |
|          ImmutableArraySpan |       2000 |         1000 |        600 |  4.550 us | 0.0900 us | 0.0842 us |  4.557 us |  1.00 |
|                             |            |              |            |           |           |           |           |       |
|                  Dictionary |      10000 |         1000 |        300 | 20.374 us | 0.3939 us | 0.4537 us | 20.217 us |  4.47 |
| UnoptimisedFrozenDictionary |      10000 |         1000 |        300 | 20.566 us | 0.4024 us | 0.5899 us | 20.400 us |  4.54 |
|   OptimisedFrozenDictionary |      10000 |         1000 |        300 |  9.967 us | 0.1540 us | 0.1365 us |  9.954 us |  2.18 |
|          ImmutableArraySpan |      10000 |         1000 |        300 |  4.553 us | 0.0871 us | 0.1037 us |  4.541 us |  1.00 |
|                             |            |              |            |           |           |           |           |       |
|                  Dictionary |      10000 |         1000 |        600 | 22.115 us | 0.3847 us | 0.3599 us | 22.153 us |  4.77 |
| UnoptimisedFrozenDictionary |      10000 |         1000 |        600 | 20.457 us | 0.1905 us | 0.1591 us | 20.463 us |  4.40 |
|   OptimisedFrozenDictionary |      10000 |         1000 |        600 | 10.571 us | 0.2073 us | 0.3685 us | 10.458 us |  2.30 |
|          ImmutableArraySpan |      10000 |         1000 |        600 |  4.609 us | 0.0893 us | 0.1097 us |  4.619 us |  1.00 |

#### Details of run

BenchmarkDotNet=v0.13.5, OS=Windows 11 (10.0.22000.1936/21H2/SunValley)
Intel Core i7-8750H CPU 2.20GHz (Coffee Lake), 1 CPU, 12 logical and 6 physical cores
.NET SDK=8.0.100-preview.4.23260.5
  [Host]     : .NET 8.0.0 (8.0.23.25905), X64 RyuJIT AVX2
  DefaultJob : .NET 8.0.0 (8.0.23.25905), X64 RyuJIT AVX2
