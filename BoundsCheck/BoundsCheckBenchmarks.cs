using BenchmarkDotNet.Attributes;
using System.Numerics;
using NUnit.Framework;

namespace DotNet8Benchmarks.BoundsCheck;

public class BoundsCheckBenchmarks
{
    private static readonly Implementations.Bounds Bounds = new(20, 50, 30, 70);
    private static readonly float XMiddle = (Bounds.Right + Bounds.Left) / 2.0f;
    private static readonly float YMiddle = (Bounds.Bottom + Bounds.Top) / 2.0f;
    /// <summary>
    /// Grid left to right, top to bottom where 4 is the center of the bounds
    /// 0 1 2
    /// 3 4 5
    /// 6 7 8
    /// </summary>
    private readonly Vector2[] _points = {
        new(XMiddle - Bounds.Width, YMiddle - Bounds.Height),
        new(XMiddle, YMiddle - Bounds.Height),
        new(XMiddle + Bounds.Width, YMiddle - Bounds.Height),
        new(XMiddle - Bounds.Width, YMiddle),
        new(XMiddle, YMiddle),
        new(XMiddle + Bounds.Width, YMiddle),
        new(XMiddle - Bounds.Width, YMiddle + Bounds.Height),
        new(XMiddle, YMiddle + Bounds.Height),
        new(XMiddle + Bounds.Width, YMiddle + Bounds.Height),
    };

    [Params(0,1,2,3,4,5,6,7,8)] public int PositionIndex;
    private Vector2 _pointToUse;

    [GlobalSetup]
    public void GlobalSetup() => _pointToUse = _points[PositionIndex];

    [Benchmark] public bool ObviousImplementation() => Bounds.ObviousImplementation(_pointToUse);
    [Benchmark] public bool ExampleFromAi() => Bounds.ExampleFromAi(_pointToUse);
    [Benchmark] public bool ExampleFromAiImproved() => Bounds.ExampleFromAiImproved(_pointToUse);


    [Test] public void ObviousImplementation([Range(0,8)] int positionIndex) => Assert.True(Bounds.ObviousImplementation(_points[positionIndex]) == (positionIndex == 4));
    [Test] public void ExampleFromAi([Range(0,8)] int positionIndex) => Assert.True(Bounds.ExampleFromAi(_points[positionIndex]) == (positionIndex == 4));
    [Test] public void ExampleFromAiImproved([Range(0,8)] int positionIndex) => Assert.True(Bounds.ExampleFromAiImproved(_points[positionIndex]) == (positionIndex == 4));

}