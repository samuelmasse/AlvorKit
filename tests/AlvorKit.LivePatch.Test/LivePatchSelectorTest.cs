namespace AlvorKit.LivePatch.Test;

/// <summary>Verifies injector provenance selection and collision policy without a native profiler.</summary>
[TestClass]
public class LivePatchSelectorTest
{
    /// <summary>Two sibling scopes can select different receivers of the same method.</summary>
    [TestMethod]
    public void ExactScopeSelectsOnlyItsOwnedReceiver()
    {
        var fixture = new ScopeFixture();
        var emberSelector = LivePatchSelector.ExactScope(fixture.EmberId);

        Assert.IsTrue(emberSelector.Matches(fixture.Ember, fixture.Graph));
        Assert.IsFalse(emberSelector.Matches(fixture.Tide, fixture.Graph));
    }

    /// <summary>A descendant selector includes nested ownership but excludes siblings.</summary>
    [TestMethod]
    public void DescendantsFollowTrackedGraphAncestry()
    {
        var fixture = new ScopeFixture();
        var nestedScope = fixture.Graph.Scope<NestedScope>(
            fixture.EmberScope,
            "nested");
        var nested = nestedScope.Get<NestedService>();
        var selector = LivePatchSelector.Descendants(fixture.EmberId);

        Assert.IsTrue(selector.Matches(fixture.Ember, fixture.Graph));
        Assert.IsTrue(selector.Matches(nested, fixture.Graph));
        Assert.IsFalse(selector.Matches(fixture.Tide, fixture.Graph));
    }

    /// <summary>Overlapping selectors are rejected instead of gaining registration-order precedence.</summary>
    [TestMethod]
    public void SlotRejectsImplicitSelectorCollision()
    {
        var fixture = new ScopeFixture();
        var slot = new LivePatchSlot(fixture.Graph);
        using var first = Trampoline();
        using var collision = Trampoline();
        slot.Add(
            1,
            LivePatchSelector.ExactScope(fixture.EmberId),
            first);

        var exception = Assert.ThrowsExactly<InvalidOperationException>(() =>
            slot.Add(
                2,
                LivePatchSelector.ExactInstance(fixture.Ember),
                collision));

        StringAssert.Contains(exception.Message, "explicit composition");
    }

    /// <summary>Ending ownership makes an exact-scope selector miss immediately.</summary>
    [TestMethod]
    public void EndingScopeStopsMatchingReceiver()
    {
        var fixture = new ScopeFixture();
        var selector = LivePatchSelector.ExactScope(fixture.EmberId);

        fixture.Graph.End(fixture.EmberScope);

        Assert.IsFalse(selector.Matches(fixture.Ember, fixture.Graph));
        Assert.IsFalse(fixture.Graph.TryGetOwner(fixture.Ember, out _));
    }

    private static InterceptionHandlerTrampoline Trampoline() =>
        InterceptionHandlerTrampolineFactory.Create(
            Method<ScopedService>(nameof(ScopedService.Calculate)),
            new ScopedHandler(),
            Method<ScopedHandler>(nameof(ScopedHandler.Run)));

    private static MethodInfo Method<T>(string name) =>
        typeof(T).GetMethod(name)
        ?? throw new InvalidOperationException($"Method '{typeof(T).FullName}.{name}' was not found.");

    private sealed class ScopeFixture
    {
        internal ScopeFixture()
        {
            var injector = new Injector();
            Graph = new(injector);
            EmberScope = Graph.Scope<TestScope>(injector, "ember");
            var tideScope = Graph.Scope<TestScope>(injector, "tide");
            Ember = EmberScope.Get<ScopedService>();
            Tide = tideScope.Get<ScopedService>();
            EmberId = Graph.GetId(EmberScope);
        }

        internal InjectorScopeGraph Graph { get; }

        internal TestScope EmberScope { get; }

        internal ScopedService Ember { get; }

        internal ScopedService Tide { get; }

        internal InjectorScopeId EmberId { get; }
    }
}

/// <summary>Marks one test patch scope.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class TestPatchAttribute : InjectorAttribute;

/// <summary>Sibling patch test scope.</summary>
[TestPatch]
public sealed class TestScope : InjectorScope<TestPatchAttribute>;

/// <summary>Marks one nested patch test scope.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public sealed class NestedPatchAttribute : InjectorAttribute;

/// <summary>Nested ownership test scope.</summary>
[NestedPatch]
public sealed class NestedScope : InjectorScope<NestedPatchAttribute>;

/// <summary>Ordinary owned test receiver.</summary>
[TestPatch]
public sealed class ScopedService
{
    public int Calculate(int value) => value * 2;
}

/// <summary>Nested owned test receiver.</summary>
[NestedPatch]
public sealed class NestedService;

/// <summary>Exact replacement used to create selector-slot trampolines.</summary>
public sealed class ScopedHandler
{
    public int Run(ScopedService receiver, int value)
    {
        _ = receiver;
        return value * 3;
    }
}
