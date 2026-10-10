using System.Runtime.CompilerServices;
using static AlvorKit.RaceFennecsComponents;
using fennecs;
using static AlvorKit.RaceFennecsUpdate1;

namespace AlvorKit;

internal static class RaceFennecsUpdate1Raw
{
    internal static void Run(RaceFennecsUpdate1.FennecsContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            fixture.query.Raw(delegate (Memory<Component1> vectors)
            {
                foreach (ref var v in vectors.Span)
                    v.Value++;
            });
        }
    }
}
