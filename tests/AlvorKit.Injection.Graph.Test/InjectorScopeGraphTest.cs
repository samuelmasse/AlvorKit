namespace AlvorKit.Injection.Graph.Test;

/// <summary>Verifies explicit scope ownership, sibling identity, and lifecycle transitions.</summary>
[TestClass]
public class InjectorScopeGraphTest
{
    /// <summary>Multiple scopes with the same type retain distinct graph identities and labels.</summary>
    [TestMethod]
    public void ScopeTracksSiblingInstances()
    {
        var injector = new Injector();
        var graph = new InjectorScopeGraph(injector, "Demo");

        var ember = graph.Scope<TestScope>(injector, "Ember");
        var tide = graph.Scope<TestScope>(injector, "Tide");

        var snapshot = graph.Snapshot();
        Assert.HasCount(3, snapshot.Nodes);
        Assert.AreEqual(graph.RootId, snapshot.Nodes[1].ParentId);
        Assert.AreEqual(graph.RootId, snapshot.Nodes[2].ParentId);
        Assert.AreEqual("Ember", snapshot.Nodes[1].Label);
        Assert.AreEqual("Tide", snapshot.Nodes[2].Label);
        Assert.AreNotEqual(snapshot.Nodes[1].Id, snapshot.Nodes[2].Id);
        Assert.AreEqual(snapshot.Nodes[1].Id, graph.GetId(ember));
        Assert.AreEqual(snapshot.Nodes[2].Id, graph.GetId(tide));
        Assert.AreSame(ember, Active(graph, snapshot.Nodes[1].Id));
        Assert.AreSame(tide, Active(graph, snapshot.Nodes[2].Id));
    }

    /// <summary>Ending a scope runs teardown once and releases executable access to the scope.</summary>
    [TestMethod]
    public void EndRunsTeardownAndReleasesScope()
    {
        var injector = new Injector();
        var graph = new InjectorScopeGraph(injector);
        var scope = graph.Scope<TestScope>(injector);
        var id = graph.Snapshot().Nodes[1].Id;
        TestScope? observed = null;

        graph.End(scope, ending => observed = ending);

        Assert.AreSame(scope, observed);
        Assert.IsFalse(graph.TryGetActiveScope(id, out _));
        var ended = graph.Snapshot(includeEnded: true).Nodes[1];
        Assert.AreEqual(InjectorScopeLifecycle.Ended, ended.Lifecycle);
    }

    /// <summary>A parent cannot end while it still owns a tracked active child.</summary>
    [TestMethod]
    public void EndRejectsActiveChild()
    {
        var injector = new Injector();
        var graph = new InjectorScopeGraph(injector);
        var parent = graph.Scope<TestScope>(injector);
        graph.Scope<NestedScope>(parent);

        var exception = Assert.ThrowsExactly<InjectorScopeGraphException>(() => graph.End(parent));

        StringAssert.Contains(exception.Message, "while child");
    }

    /// <summary>Temporary graph scopes end even when their operation throws.</summary>
    [TestMethod]
    public void RunEndsScopeAfterFailure()
    {
        var injector = new Injector();
        var graph = new InjectorScopeGraph(injector);

        Assert.ThrowsExactly<InvalidOperationException>(
            () => graph.Run<TestScope>(injector, _ => throw new InvalidOperationException("demo")));

        var child = graph.Snapshot(includeEnded: true).Nodes[1];
        Assert.AreEqual(InjectorScopeLifecycle.Ended, child.Lifecycle);
    }

    /// <summary>Constructed, added, and bound references retain exact owning-scope provenance.</summary>
    [TestMethod]
    public void TracksEveryOwnedInstancePath()
    {
        var injector = new Injector();
        var graph = new InjectorScopeGraph(injector);
        var scope = graph.Scope<TestScope>(injector, "owned");
        var scopeId = graph.GetId(scope);
        var constructed = scope.Get<TestService>();
        var added = new TestAddedService();
        var bound = new TestBoundService();

        scope.Add(added);
        scope.Bind(bound);

        AssertOwner(graph, constructed, scopeId);
        AssertOwner(graph, added, scopeId);
        AssertOwner(graph, bound, scopeId);
    }

    /// <summary>The ending callback runs while provenance remains queryable and before teardown.</summary>
    [TestMethod]
    public void ScopeEndingRetainsOwnershipUntilTeardown()
    {
        var injector = new Injector();
        var graph = new InjectorScopeGraph(injector);
        var scope = graph.Scope<TestScope>(injector);
        var service = scope.Get<TestService>();
        var expected = graph.GetId(scope);
        var notified = false;

        graph.ScopeEnding += ending =>
        {
            Assert.AreEqual(expected, ending.Id);
            Assert.AreSame(scope, ending.Scope);
            AssertOwner(graph, service, expected);
            notified = true;
        };

        graph.End(scope);

        Assert.IsTrue(notified);
        Assert.IsFalse(graph.TryGetOwner(service, out _));
    }

    private static InjectorScope Active(InjectorScopeGraph graph, InjectorScopeId id)
    {
        Assert.IsTrue(graph.TryGetActiveScope(id, out var scope));
        return scope;
    }

    private static void AssertOwner(
        InjectorScopeGraph graph,
        object instance,
        InjectorScopeId expected)
    {
        Assert.IsTrue(graph.TryGetOwner(instance, out var actual));
        Assert.AreEqual(expected, actual);
    }
}

/// <summary>Marks services owned by one test scope.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class TestAttribute : InjectorAttribute;

/// <summary>Test scope used to model sibling lifetimes.</summary>
[Test]
public class TestScope : InjectorScope<TestAttribute>;

/// <summary>Marks services owned by a nested test scope.</summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class NestedAttribute : InjectorAttribute;

/// <summary>Test scope used to model an active child lifetime.</summary>
[Nested]
public class NestedScope : InjectorScope<NestedAttribute>;

/// <summary>Constructed service used by ownership tests.</summary>
[Test]
public sealed class TestService;

/// <summary>Added service used by ownership tests.</summary>
[Test]
public sealed class TestAddedService;

/// <summary>Bound service surface used by ownership tests.</summary>
public interface ITestBoundService;

/// <summary>Bound service used by ownership tests.</summary>
[Test]
public sealed class TestBoundService : ITestBoundService;
