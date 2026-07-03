namespace AlvorKit.Engine.Test;

[TestClass]
public sealed class RootWindowLoopWrapperTest
{
    /// <summary>Every public window-loop wrapper has a root-scoped subclass with the expected name.</summary>
    [TestMethod]
    public void PublicWindowLoopWrappers_HaveRootEquivalents()
    {
        var wrappers = typeof(WindowLoop).Assembly.GetTypes()
            .Where(IsPublicWindowLoopWrapper)
            .OrderBy(x => x.Name)
            .ToArray();

        Assert.IsTrue(wrappers.Length > 0, "Expected at least one public window-loop wrapper.");
        Assert.IsTrue(wrappers.Contains(typeof(WindowCanvas)), "WindowCanvas must stay covered by the root-equivalent check.");

        foreach (var wrapper in wrappers)
        {
            var rootName = RootName(wrapper);
            var rootType = typeof(RootScope).Assembly.GetType($"AlvorKit.Engine.{rootName}");

            Assert.IsNotNull(rootType, $"Missing root equivalent '{rootName}' for '{wrapper.FullName}'.");
            Assert.IsTrue(rootType.IsDefined(typeof(RootAttribute), false), $"'{rootName}' must be marked [Root].");
            Assert.IsTrue(wrapper.IsAssignableFrom(rootType), $"'{rootName}' must inherit from '{wrapper.FullName}'.");

            var root = Activator.CreateInstance(rootType, new WindowLoop(new FakeWindowHost()));
            Assert.IsInstanceOfType(root, wrapper);
        }
    }

    private static bool IsPublicWindowLoopWrapper(Type type) =>
        type.IsClass &&
        type.IsPublic &&
        type.Namespace == "AlvorKit.Windowing" &&
        type.GetConstructors().Any(x => x.GetParameters().FirstOrDefault()?.ParameterType == typeof(WindowLoop));

    private static string RootName(Type wrapper) =>
        wrapper.Name.StartsWith("Window", StringComparison.Ordinal)
            ? $"Root{wrapper.Name["Window".Length..]}"
            : $"Root{wrapper.Name}";
}
