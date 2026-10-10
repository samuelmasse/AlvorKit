namespace AlvorKit;

internal static class RaceAlvorCreate2ArchetypalReusedBuilder
{
    internal static EntPtr Run(RaceAlvorKitBaseContext fixture, int entCount, int passes)
    {
        EntPtr last = default;
        var builder = fixture.Arena.AllocArchetypal<RaceAlvorComponents>()
            .With<int, RaceAlvorComponents.Component1>(0)
            .With<int, RaceAlvorComponents.Component2>(0);

        for (var pass = 0; pass < passes; pass++)
        {
            for (var i = 0; i < entCount; i++)
                last = builder.Create();
        }

        return last;
    }
}
