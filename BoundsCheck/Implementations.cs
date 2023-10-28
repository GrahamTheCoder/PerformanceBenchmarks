using System.Numerics;
using System.Runtime.CompilerServices;

namespace DotNet8Benchmarks.BoundsCheck;

public static class Implementations
{

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ExampleFromAiImproved(in this Bounds bounds, in Vector2 point)
    {
        unchecked
        {
            return (uint) (point.X - bounds.Left) <= (uint) bounds.Width &&
                   (uint) (point.Y - bounds.Top) <= (uint) bounds.Height;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool ExampleFromAi(in this Bounds bounds, in Vector2 point)
    {
        unchecked
        {
            var insideX= (uint) (point.X - bounds.Left) <= (uint) bounds.Width;
            var insideY = (uint) (point.Y - bounds.Top) <= (uint) bounds.Height;
            return insideX & insideY;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]

    public static bool ObviousImplementation(in this Bounds bounds, in Vector2 point)
    {
        return bounds.Left <= point.X && point.X <= bounds.Right && bounds.Top <= point.Y && point.Y <= bounds.Bottom;
    }

    public readonly struct Bounds
    {

        public readonly int Left;
        public readonly int Right;
        public readonly int Top;
        public readonly int Bottom;
        public readonly int Width;
        public readonly int Height;

        public Bounds(int Left, int Right, int Top, int Bottom)
        {
            this.Left = Left;
            this.Right = Right;
            this.Top = Top;
            this.Bottom = Bottom;
            Width = Right - Left;
            Height = Bottom - Top;
        }
    }
}