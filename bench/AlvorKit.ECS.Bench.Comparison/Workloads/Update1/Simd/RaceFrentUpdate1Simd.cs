using System.Runtime.InteropServices;
using Frent;
using Frent.Systems;
using static AlvorKit.RaceFrentBaseContext;
using static AlvorKit.RaceFrentUpdate1;

namespace AlvorKit;

internal static class RaceFrentUpdate1Simd
{
    internal static void Run(RaceFrentUpdate1.FrentContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            Vector256<int> sum = Vector256.Create(1);

            foreach (ChunkTuple<Component1> chunk in fixture.Query.EnumerateChunks<Component1>())
            {
                int len = chunk.Span.Length - (chunk.Span.Length & 0b111);
                Span<Vector256<int>> ints = MemoryMarshal.Cast<Component1, Vector256<int>>(chunk.Span.Slice(0, len));

                for (int i = 0; i < ints.Length; i++)
                    ints[i] += sum;

                for (int i = len; i < chunk.Span.Length; i++)
                    chunk.Span[i].Value++;
            }
        }
    }
}
