using static AlvorKit.RaceFrifloEngineComponents;
using Friflo.Engine.ECS;
using static AlvorKit.RaceFrifloUpdate2;

namespace AlvorKit;

internal static class RaceFrifloUpdate2SIMDScalar
{
    internal static void Run(RaceFrifloUpdate2.FrifloEngineEcsContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            foreach (var (chunk1, chunk2, _) in fixture.queryTwo.Chunks)
            {
                var c1 = System.Runtime.InteropServices.MemoryMarshal.Cast<Component1, int>(chunk1.Span);
                var c2 = System.Runtime.InteropServices.MemoryMarshal.Cast<Component2, int>(chunk2.Span);
                var offset = 0;

                for (; offset + Vector256<int>.Count <= c1.Length; offset += Vector256<int>.Count)
                {
                    var v1 = Vector256.LoadUnsafe(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(c1), (nuint)offset);
                    var v2 = Vector256.LoadUnsafe(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(c2), (nuint)offset);
                    var result = v1 + v2;
                    result.StoreUnsafe(ref System.Runtime.InteropServices.MemoryMarshal.GetReference(c1), (nuint)offset);
                }
                for (; offset < c1.Length; offset++)
                    c1[offset] += c2[offset];
            }
        }
    }
}
