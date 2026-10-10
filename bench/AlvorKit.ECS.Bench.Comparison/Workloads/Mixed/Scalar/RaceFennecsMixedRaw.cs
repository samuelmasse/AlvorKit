using static AlvorKit.RaceFennecsComponents;
using fennecs;
using static AlvorKit.RaceFennecsMixed;

namespace AlvorKit;

internal static class RaceFennecsMixedRaw
{
    internal static void Run(RaceFennecsMixed.FennecsContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            fixture.query.Raw(delegate (Memory<Component1> c1v, Memory<Component2> c2v)
            {
                var c1vs = c1v.Span;
                var c2vs = c2v.Span;

                for (int i = 0; i < c1vs.Length; ++i)
                {
                    ref Component1 c1 = ref c1vs[i];
                    c1.Value += c2vs[i].Value;
                }
            });
        }
    }
}
