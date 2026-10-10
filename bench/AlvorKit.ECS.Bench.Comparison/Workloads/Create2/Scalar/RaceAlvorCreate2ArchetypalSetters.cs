namespace AlvorKit;

internal static class RaceAlvorCreate2ArchetypalSetters
{
    internal static void Run(RaceAlvorKitBaseContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (int i = 0; i < entCount; ++i)
            {
                EntPtr ent = fixture.Arena.Alloc();
                ent.Component1 = 0;
                ent.Component2 = 0;
            }
        }
    }
}
