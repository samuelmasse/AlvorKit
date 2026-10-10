using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using static AlvorKit.RaceAlvorUpdate2;

namespace AlvorKit;

internal static class RaceAlvorUpdate2QuerySIMD
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
                int vectorLength = component1.Length - component1.Length % Vector256<int>.Count;
                Span<Vector256<int>> vectors1 = MemoryMarshal.Cast<int, Vector256<int>>(component1[..vectorLength]);
                Span<Vector256<int>> vectors2 = MemoryMarshal.Cast<int, Vector256<int>>(component2[..vectorLength]);

                for (int i = 0; i < vectors1.Length; i++)
                    vectors1[i] += vectors2[i];

                for (int i = vectorLength; i < component1.Length; i++)
                    component1[i] += component2[i];
            }
        }
    }
}
