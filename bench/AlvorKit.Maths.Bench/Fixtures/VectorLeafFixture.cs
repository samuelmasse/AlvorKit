namespace AlvorKit;

internal class VectorLeafFixture
{
    private const int BatchSize = 4096;

    private readonly Vec4[] alvorInput = new Vec4[4096];
    private readonly Vec4[] alvorOutput = new Vec4[4096];
    private readonly System.Numerics.Vector4[] systemInput = new System.Numerics.Vector4[4096];
    private readonly System.Numerics.Vector4[] systemOutput = new System.Numerics.Vector4[4096];
    internal Vec4[] AlvorInput => alvorInput;
    internal Vec4[] AlvorOutput => alvorOutput;
    internal System.Numerics.Vector4[] SystemInput => systemInput;
    internal System.Numerics.Vector4[] SystemOutput => systemOutput;

    internal VectorLeafFixture()
    {
        for (var i = 0; i < BatchSize; i++)
        {
            var x = (((i * 17) % 101) - 50) * 0.03125f;
            AlvorInput[i] = (x - 4.5f, x + 2.25f, x - 1.5f, x + 7.75f);
            SystemInput[i] = AlvorInput[i];
        }
    }
}
