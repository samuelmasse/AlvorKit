namespace AlvorKit;

internal class InliningHintFixture
{
    private const int BatchSize = 4096;

    private readonly Vec3[] left = new Vec3[4096];
    private readonly Vec3[] right = new Vec3[4096];
    private readonly Vec3[] output = new Vec3[4096];
    internal Vec3[] Left => left;
    internal Vec3[] Right => right;
    internal Vec3[] Output => output;

    internal InliningHintFixture()
    {
        for (var i = 0; i < BatchSize; i++)
        {
            var x = (((i * 17) % 101) - 50) * 0.03125f;
            Left[i] = (x + 1.25f, x + 2.5f, x + 3.75f);
            Right[i] = (x * 0.25f + 4.5f, x * -0.5f + 2.25f, x * 0.75f - 1.5f);
        }
    }
}
