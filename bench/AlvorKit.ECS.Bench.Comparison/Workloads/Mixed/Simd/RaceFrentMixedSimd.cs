using System.Runtime.InteropServices;
using Frent;
using Frent.Systems;
using static AlvorKit.RaceFrentBaseContext;
using static AlvorKit.RaceFrentMixed;

namespace AlvorKit;

internal static class RaceFrentMixedSimd
{
    internal static void Run(RaceFrentMixed.FrentContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            foreach (var (s1, s2) in fixture.Query.EnumerateChunks<Component1, Component2>())
            {
                int len = s1.Length - (s1.Length & 0b111);
                Span<Vector256<int>> ints = MemoryMarshal.Cast<Component1, Vector256<int>>(s1.Slice(0, len));
                Span<Vector256<int>> a = MemoryMarshal.Cast<Component2, Vector256<int>>(s2.Slice(0, len))[..ints.Length];

                for (int i = 0; i < ints.Length; i++)
                    ints[i] += a[i];

                for (int i = len; i < s1.Length; i++)
                    s1[i].Value += s2[i].Value;
            }
        }
    }
}
