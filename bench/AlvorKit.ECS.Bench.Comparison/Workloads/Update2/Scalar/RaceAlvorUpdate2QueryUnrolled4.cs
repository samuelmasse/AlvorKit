using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static AlvorKit.RaceAlvorUpdate2;

namespace AlvorKit;

internal static class RaceAlvorUpdate2QueryUnrolled4
{
    internal static void Run(RaceAlvorUpdate2.AlvorKitContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            var query = fixture.Arena.QueryArchetypal<RaceAlvorComponents>()
                .With<int, RaceAlvorComponents.Component1>()
                .With<int, RaceAlvorComponents.Component2>();

            foreach (var chunk in query)
            {
                Span<int> component1 = chunk.Get<int, RaceAlvorComponents.Component1>();
                Span<int> component2 = chunk.Get<int, RaceAlvorComponents.Component2>();
                ref int component1Base = ref MemoryMarshal.GetReference(component1);
                ref int component2Base = ref MemoryMarshal.GetReference(component2);
                nint count = component1.Length;
                nint unrolledCount = count & ~3;
                nint i = 0;

                for (; i < unrolledCount; i += 4)
                {
                    Unsafe.Add(ref component1Base, i) += Unsafe.Add(ref component2Base, i);
                    Unsafe.Add(ref component1Base, i + 1) += Unsafe.Add(ref component2Base, i + 1);
                    Unsafe.Add(ref component1Base, i + 2) += Unsafe.Add(ref component2Base, i + 2);
                    Unsafe.Add(ref component1Base, i + 3) += Unsafe.Add(ref component2Base, i + 3);
                }
                for (; i < count; i++)
                    Unsafe.Add(ref component1Base, i) += Unsafe.Add(ref component2Base, i);
            }
        }
    }
}
