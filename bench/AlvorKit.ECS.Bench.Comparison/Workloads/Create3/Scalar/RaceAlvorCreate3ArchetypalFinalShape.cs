namespace AlvorKit;

internal static class RaceAlvorCreate3ArchetypalFinalShape
{
    internal static void Run(RaceAlvorKitBaseContext fixture, int entCount, int passes)
    {
        for (var pass = 0; pass < passes; pass++)
        {
            for (int i = 0; i < entCount; ++i)
            {
                fixture.Arena.AllocArchetypal<RaceAlvorComponents>()
                    .With<int, RaceAlvorComponents.Component1>(0)
                    .With<int, RaceAlvorComponents.Component2>(0)
                    .With<int, RaceAlvorComponents.Component3>(0).Create();
            }
        }
    }
}
