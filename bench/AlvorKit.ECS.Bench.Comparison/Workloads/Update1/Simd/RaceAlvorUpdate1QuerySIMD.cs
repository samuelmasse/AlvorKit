using System.Runtime.InteropServices;
using static AlvorKit.RaceAlvorUpdate1;

namespace AlvorKit;

internal static class RaceAlvorUpdate1QuerySIMD
{
    internal static void Run(RaceAlvorUpdate1.AlvorKitContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            Vector256<int> one = Vector256.Create(1);
            var query = fixture.Arena.QueryArchetypal<RaceAlvorComponents>().With<int, RaceAlvorComponents.Component1>();

            foreach (var chunk in query)
            {
                Span<int> components = chunk.Get<int, RaceAlvorComponents.Component1>();
                int vectorLength = components.Length - components.Length % Vector256<int>.Count;
                Span<Vector256<int>> vectors = MemoryMarshal.Cast<int, Vector256<int>>(components[..vectorLength]);

                for (int i = 0; i < vectors.Length; i++)
                    vectors[i] += one;

                for (int i = vectorLength; i < components.Length; i++)
                    components[i]++;
            }
        }
    }
}
