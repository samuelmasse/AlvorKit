namespace AlvorKit;

internal class FloatValueSemanticsFixture
{
    private const int BatchSize = 4096;

    private readonly FloatValueSemanticsFixtureAlvor alvor = new();
    private readonly FloatValueSemanticsFixtureNumerics numerics = new();
    private readonly FloatValueSemanticsFixtureShared shared = new();
    internal FloatValueSemanticsFixtureAlvor Alvor => alvor;
    internal FloatValueSemanticsFixtureNumerics Numerics => numerics;
    internal FloatValueSemanticsFixtureShared Shared => shared;

    internal FloatValueSemanticsFixture()
    {
        for (var i = 0; i < BatchSize; i++)
        {
            var x = (((i * 17) % 101) - 50) * 0.03125f;
            Alvor.AlvorLeft2[i] = (x + 1.25f, x - 2.5f);
            Alvor.AlvorRight2[i] = (x + 3.75f, x + 4.5f);
            Alvor.AlvorLeft3[i] = (x + 1.25f, x - 2.5f, x + 3.125f);
            Alvor.AlvorRight3[i] = (x + 3.75f, x + 4.5f, x - 1.875f);
            Alvor.AlvorLeft4[i] = (x + 1.25f, x - 2.5f, x + 3.125f, x + 0.75f);
            Alvor.AlvorRight4[i] = (x + 3.75f, x + 4.5f, x - 1.875f, x + 2.25f);
            Numerics.SystemLeft2[i] = Alvor.AlvorLeft2[i];
            Numerics.SystemRight2[i] = Alvor.AlvorRight2[i];
            Numerics.SystemLeft3[i] = Alvor.AlvorLeft3[i];
            Numerics.SystemRight3[i] = Alvor.AlvorRight3[i];
            Numerics.SystemLeft4[i] = Alvor.AlvorLeft4[i];
            Numerics.SystemRight4[i] = Alvor.AlvorRight4[i];
        }
    }
}
