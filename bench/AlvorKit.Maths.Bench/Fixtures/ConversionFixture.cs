namespace AlvorKit;

internal class ConversionFixture
{
    private const int BatchSize = 4096;

    private readonly Vec3d[] doubles = new Vec3d[4096];
    private readonly Vec3[] floats = new Vec3[4096];
    private readonly Vec3[] floatOutput = new Vec3[4096];
    private readonly Vec3d[] doubleOutput = new Vec3d[4096];
    private readonly Vec2i[] ints = new Vec2i[4096];
    private readonly Vec2u[] uintOutput = new Vec2u[4096];
    internal Vec3d[] Doubles => doubles;
    internal Vec3[] Floats => floats;
    internal Vec3[] FloatOutput => floatOutput;
    internal Vec3d[] DoubleOutput => doubleOutput;
    internal Vec2i[] Ints => ints;
    internal Vec2u[] UintOutput => uintOutput;

    internal ConversionFixture()
    {
        for (var i = 0; i < BatchSize; i++)
        {
            var x = (((i * 17) % 101) - 50) * 0.03125;
            Doubles[i] = (x + 1.25, x - 2.5, x + 3.125);
            Floats[i] = ((float)(x + 1.25), (float)(x - 2.5), (float)(x + 3.125));
            Ints[i] = (i - 2_048, 2_048 - (i * 3));
        }
    }
}
