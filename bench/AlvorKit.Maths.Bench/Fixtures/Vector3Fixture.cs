namespace AlvorKit;

internal class Vector3Fixture
{
    private const int BatchSize = 4096;

    private readonly Vector3FixtureAlvor alvor = new();
    private readonly Vector3FixtureNumerics numerics = new();
    private readonly Vector3FixtureShared shared = new();
    internal Vector3FixtureAlvor Alvor => alvor;
    internal Vector3FixtureNumerics Numerics => numerics;
    internal Vector3FixtureShared Shared => shared;

    internal Vector3Fixture()
    {
        for (var index = 0; index < BatchSize; index++)
        {
            Shared.Amount[index] = (index % 101) / 100f;
            Shared.Scalar[index] = 0.75f + ((index % 17) * 0.03125f);
        }

        for (var index = 0; index < BatchSize; index++)
        {
            var x = ((((index * 17) % 101) - 50) * 0.03125f);
            Alvor.AlvorLeft[index] = (x + 1.25f, x - 2.5f, x + 3.125f);
            Alvor.AlvorRight[index] = (x + 3.75f, x + 4.5f, x - 1.875f);
            Numerics.SystemLeft[index] = new(x + 1.25f, x - 2.5f, x + 3.125f);
            Numerics.SystemRight[index] = new(x + 3.75f, x + 4.5f, x - 1.875f);
        }
    }
}
