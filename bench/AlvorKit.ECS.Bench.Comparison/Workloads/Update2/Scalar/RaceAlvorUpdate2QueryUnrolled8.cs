using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static AlvorKit.RaceAlvorUpdate2;

namespace AlvorKit;

internal static class RaceAlvorUpdate2QueryUnrolled8
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
                ref int component1Current = ref MemoryMarshal.GetReference(component1);
                ref int component2Current = ref MemoryMarshal.GetReference(component2);
                nint remaining = component1.Length;

                while (remaining >= 8)
                {
                    Unsafe.Add(ref component1Current, 0) += Unsafe.Add(ref component2Current, 0);
                    Unsafe.Add(ref component1Current, 1) += Unsafe.Add(ref component2Current, 1);
                    Unsafe.Add(ref component1Current, 2) += Unsafe.Add(ref component2Current, 2);
                    Unsafe.Add(ref component1Current, 3) += Unsafe.Add(ref component2Current, 3);
                    Unsafe.Add(ref component1Current, 4) += Unsafe.Add(ref component2Current, 4);
                    Unsafe.Add(ref component1Current, 5) += Unsafe.Add(ref component2Current, 5);
                    Unsafe.Add(ref component1Current, 6) += Unsafe.Add(ref component2Current, 6);
                    Unsafe.Add(ref component1Current, 7) += Unsafe.Add(ref component2Current, 7);
                    component1Current = ref Unsafe.Add(ref component1Current, 8);
                    component2Current = ref Unsafe.Add(ref component2Current, 8);
                    remaining -= 8;
                }
                while (remaining != 0)
                {
                    component1Current += component2Current;
                    component1Current = ref Unsafe.Add(ref component1Current, 1);
                    component2Current = ref Unsafe.Add(ref component2Current, 1);
                    remaining--;
                }
            }
        }
    }
}
