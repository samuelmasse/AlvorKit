namespace AlvorKit;

internal class Vector2FixtureAlvor
{
    private readonly Vec2[] alvorLeft = new Vec2[4096];
    private readonly Vec2[] alvorRight = new Vec2[4096];
    private readonly Vec2[] alvorOutput = new Vec2[4096];
    internal Vec2[] AlvorLeft => alvorLeft;
    internal Vec2[] AlvorRight => alvorRight;
    internal Vec2[] AlvorOutput => alvorOutput;
}
