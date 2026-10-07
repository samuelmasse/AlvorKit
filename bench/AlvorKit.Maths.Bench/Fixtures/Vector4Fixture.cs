namespace AlvorKit;

internal class Vector4Fixture
{
    private const int BatchSize = 4096;

    private readonly Vector4FixtureAlvor alvor = new();
    private readonly Vector4FixtureNumerics numerics = new();
    private readonly Vector4FixtureShared shared = new();
    internal Vector4FixtureAlvor Alvor => alvor;
    internal Vector4FixtureNumerics Numerics => numerics;
    internal Vector4FixtureShared Shared => shared;

    internal Vector4Fixture()
    {
        for (var index = 0; index < BatchSize; index++)
        {
            Shared.Amount[index] = (index % 101) / 100f;
            Shared.Scalar[index] = 0.75f + ((index % 17) * 0.03125f);
        }

        for (var index = 0; index < BatchSize; index++)
        {
            var x = ((((index * 17) % 101) - 50) * 0.03125f);
            Alvor.AlvorLeft[index] = (x + 1.25f, x - 2.5f, x + 3.125f, x + 0.75f);
            Alvor.AlvorRight[index] = (x + 3.75f, x + 4.5f, x - 1.875f, x + 2.25f);
            Numerics.SystemLeft[index] = new(x + 1.25f, x - 2.5f, x + 3.125f, x + 0.75f);
            Numerics.SystemRight[index] = new(x + 3.75f, x + 4.5f, x - 1.875f, x + 2.25f);
        }
    }
}
