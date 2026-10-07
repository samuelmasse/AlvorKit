namespace AlvorKit;

internal class EqualityComparerFixture
{
    private const int BatchSize = 4096;

    private readonly Vec2i[] left2 = new Vec2i[4096];
    private readonly Vec2i[] right2 = new Vec2i[4096];
    private readonly Vec3i[] left3 = new Vec3i[4096];
    private readonly Vec3i[] right3 = new Vec3i[4096];
    private readonly int[] output = new int[4096];
    internal Vec2i[] Left2 => left2;
    internal Vec2i[] Right2 => right2;
    internal Vec3i[] Left3 => left3;
    internal Vec3i[] Right3 => right3;
    internal int[] Output => output;

    internal EqualityComparerFixture()
    {
        for (var i = 0; i < BatchSize; i++)
        {
            Left2[i] = (i - 2_048, 2_048 - (i * 3));
            Right2[i] = i % 8 == 0 ? Left2[i] : Left2[i] + Vec2i.One;
            Left3[i] = (i - 2_048, 2_048 - (i * 3), (i * 7) - 1_024);
            Right3[i] = i % 8 == 0 ? Left3[i] : Left3[i] + Vec3i.One;
        }
    }
}
