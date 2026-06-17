Console.WriteLine("AlvorKit.Mocking demo");

var renderer = Mock.Create<IRenderer>();
Mock.When(() => renderer.Draw("player", Arg.Any<int>())).Return(true);

Expect(renderer.Draw("player", 3), "interface mock should match any layer for the player sprite");
Expect(!renderer.Draw("enemy", 3), "interface mock should leave unmatched calls at the default value");

var resizeCount = 0;
renderer.Resized += size =>
{
    resizeCount++;
    Expect(size.Width == 1280 && size.Height == 720, "raised event should carry the supplied size");
};

Mock.Raise(() => renderer.Resized += null, new SurfaceSize(1280, 720));
Expect(resizeCount == 1, "mocked event should invoke attached handlers");

var mouse = Mock.Create<MouseInput>();
Mock.When(() => mouse.IsPressed(MouseButton.Secondary)).Return(true);
Mock.When(() => mouse.Axis(Arg.Match<int>(axis => axis > 0))).Return(0.75f);

Expect(mouse.IsPressed(MouseButton.Secondary), "class mock should return configured button state");
Expect(!mouse.IsPressed(MouseButton.Primary), "class mock should return defaults for unmatched methods");
Expect(mouse.Axis(2) == 0.75f, "argument matcher should accept positive axes");
Expect(mouse.Axis(-1) == 0f, "argument matcher should reject negative axes");

var counter = new Counter();
Mock.Instance(counter);
Mock.When(() => counter.Next()).Return(42);

Expect(counter.Next() == 42, "partial instance mock should override configured methods");
Expect(counter.Current == 0, "configured partial calls should not advance original state");

var formatter = Mock.Create<GenericFormatter>();
Mock.Generic(formatter.Format<int>);
Mock.When(() => formatter.Format(5)).Return("five");

Expect(formatter.Format(5) == "five", "constructed generic methods should be configurable after Mock.Generic");
Expect(formatter.Format("raw") == "raw", "other constructed generic methods should keep their original implementation");

Console.WriteLine("All checks passed.");

// Fail fast so the demo doubles as a simple executable smoke check.
static void Expect(bool condition, string message)
{
    if (!condition)
        throw new InvalidOperationException(message);
}
