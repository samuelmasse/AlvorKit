using static AlvorKit.RaceEntitasContext;

namespace AlvorKit;

internal static class RaceEntitasMixedGroupDirect
{
    internal static void Run(RaceEntitasUpdateFixture fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            foreach (var ent in fixture.Group.GetEntities())
            {
                var first = (Component1)ent.GetComponent(0);
                first.Value += ((Component2)ent.GetComponent(1)).Value;
            }
        }
    }
}
