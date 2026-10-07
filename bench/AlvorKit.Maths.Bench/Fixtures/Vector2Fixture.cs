namespace AlvorKit;

internal class Vector2Fixture
{
    private const int BatchSize = 4096;

    private readonly Vector2FixtureAlvor alvor = new();
    private readonly Vector2FixtureNumerics numerics = new();
    private readonly Vector2FixtureShared shared = new();
    internal Vector2FixtureAlvor Alvor => alvor;
    internal Vector2FixtureNumerics Numerics => numerics;
    internal Vector2FixtureShared Shared => shared;

    internal Vector2Fixture()
    {
        for (var index = 0; index < BatchSize; index++)
        {
            Shared.Amount[index] = (index % 101) / 100f;
            Shared.Scalar[index] = 0.75f + ((index % 17) * 0.03125f);
        }

        for (var index = 0; index < BatchSize; index++)
        {
            var x = ((((index * 17) % 101) - 50) * 0.03125f);
            Alvor.AlvorLeft[index] = (x + 1.25f, x - 2.5f);
            Alvor.AlvorRight[index] = (x + 3.75f, x + 4.5f);
            Numerics.SystemLeft[index] = new(x + 1.25f, x - 2.5f);
            Numerics.SystemRight[index] = new(x + 3.75f, x + 4.5f);
        }
    }
}
