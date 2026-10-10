using static AlvorKit.RaceEntitasContext;

namespace AlvorKit;

internal static class RaceEntitasCreate3Default
{
    internal static void Run(RaceEntitasContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < entCount; i++)
            {
                var ent = fixture.World.CreateEntity();
                var c1 = ent.CreateComponent<Component1>(0);
                c1.Value = 0;
                ent.AddComponent(0, c1);
                var c2 = ent.CreateComponent<Component2>(1);
                c2.Value = 0;
                ent.AddComponent(1, c2);
                var c3 = ent.CreateComponent<Component3>(2);
                c3.Value = 0;
                ent.AddComponent(2, c3);
            }
        }
    }
}
