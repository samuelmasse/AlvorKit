namespace AlvorKit;

internal class Vector4FixtureAlvor
{
    private readonly Vec4[] alvorLeft = new Vec4[4096];
    private readonly Vec4[] alvorRight = new Vec4[4096];
    private readonly Vec4[] alvorOutput = new Vec4[4096];
    internal Vec4[] AlvorLeft => alvorLeft;
    internal Vec4[] AlvorRight => alvorRight;
    internal Vec4[] AlvorOutput => alvorOutput;
}
