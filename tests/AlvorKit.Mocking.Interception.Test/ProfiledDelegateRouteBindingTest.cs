namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Verifies retirement waits for immutable route delegate snapshots.</summary>
[TestClass]
public sealed class ProfiledDelegateRouteBindingTest
{
    [TestMethod]
    public async Task RetireAndDrain_WaitsForBlockedInFlightDelegate()
    {
        using var entered = new ManualResetEventSlim();
        using var release = new ManualResetEventSlim();
        var route = new MockInterceptionRoute("blocked-construction");
        void blocked()
        {
            entered.Set();
            Assert.IsTrue(release.Wait(TimeSpan.FromSeconds(10)));
        }
        var binding = new ProfiledDelegateRouteBinding<Action>(
            route,
blocked,
blocked);

        Task invocation = Task.Run(() =>
        {
            Assert.IsTrue(binding.TryAcquire(out IDisposable? lease));
            using (lease)
                binding.Original();
        });
        Assert.IsTrue(entered.Wait(TimeSpan.FromSeconds(10)));

        Task retirement = Task.Run(binding.RetireAndDrain);
        await Task.Delay(50);
        Assert.IsFalse(retirement.IsCompleted);

        release.Set();
        await Task.WhenAll(invocation, retirement);
        Assert.IsFalse(binding.TryAcquire(out _));
    }

    [TestMethod]
    public async Task RetiredRoute_PreservesOriginalForCallerPausedBeforeEntry()
    {
        using var resumeRouteEntry = new ManualResetEventSlim();
        var route = new MockInterceptionRoute("pre-route-construction");
        ProfiledConstructionFactoryTarget.Reset();
        ProfiledConstructionFactoryRoute.Bind(
            route,
            value => new(value),
            value => new(value + 100));

        Task<ProfiledConstructionFactoryTarget> oldCaller = Task.Run(() =>
        {
            Assert.IsTrue(
                resumeRouteEntry.Wait(TimeSpan.FromSeconds(10)));
            return ProfiledConstructionFactoryRoute.Invoke(17);
        });

        ProfiledConstructionFactoryRoute.Clear();
        resumeRouteEntry.Set();

        ProfiledConstructionFactoryTarget result = await oldCaller;
        Assert.AreEqual(17, result.Value);
        Assert.AreEqual(1, ProfiledConstructionFactoryTarget.ConstructorCalls);
    }

    /// <summary>Retirement releases a collectible wrapper while preserving the exact original fallback.</summary>
    [TestMethod]
    public void RetiredBinding_ReleasesCollectibleWrapperAndKeepsOriginal()
    {
        (
            ProfiledDelegateRouteBinding<Action> binding,
            WeakReference context) = RetireCollectibleWrapper();

        CollectUntilDead(context);

        Assert.IsFalse(
            context.IsAlive,
            "The retired route retained its collectible active wrapper.");
        CollectibleRouteFallback.InvocationCount = 0;
        binding.Original();
        Assert.AreEqual(1, CollectibleRouteFallback.InvocationCount);
        GC.KeepAlive(binding);
    }

    /// <summary>Constructed Mocking metadata is weakly owned by its collectible generic argument.</summary>
    [TestMethod]
    public void ConstructedRoute_GlobalCachesDoNotRootCollectibleContext()
    {
        WeakReference context = BindCollectibleConstruction();

        CollectUntilDead(context);

        Assert.IsFalse(
            context.IsAlive,
            "A process-global Mocking route cache retained the collectible " +
            "generic construction.");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static (
        ProfiledDelegateRouteBinding<Action> Binding,
        WeakReference Context) RetireCollectibleWrapper()
    {
        var context = new AssemblyLoadContext(
            "profiled-route-wrapper",
            isCollectible: true);
        var contextReference = new WeakReference(context);
        Assembly assembly = context.LoadFromAssemblyPath(
            typeof(ProfiledDelegateRouteBindingTest).Assembly.Location);
        Type marker = assembly.GetType(
            typeof(CollectibleRouteMarker).FullName!,
            throwOnError: true)!;
        Action wrapper = marker.GetMethod(
            nameof(CollectibleRouteMarker.Wrapper))!
            .CreateDelegate<Action>();
        var binding = new ProfiledDelegateRouteBinding<Action>(
            new("collectible-wrapper"),
            CollectibleRouteFallback.Invoke,
            wrapper);

        binding.RetireAndDrain();
        context.Unload();
        return (binding, contextReference);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference BindCollectibleConstruction()
    {
        var context = new AssemblyLoadContext(
            "profiled-constructed-route",
            isCollectible: true);
        var contextReference = new WeakReference(context);
        Assembly assembly = context.LoadFromAssemblyPath(
            typeof(TempWorkspace).Assembly.Location);
        Type marker = assembly.GetType(
            typeof(TempWorkspace).FullName!,
            throwOnError: true)!;
        Type targetType = typeof(CollectibleGenericRouteTarget<>)
            .MakeGenericType(marker);
        MethodInfo operation = targetType.GetMethod(
            nameof(CollectibleGenericRouteTarget<>.Invoke))!;
        MethodInfo originalMethod =
            typeof(CollectibleGenericRouteOriginal)
                .GetMethod(
                    nameof(CollectibleGenericRouteOriginal.Invoke))!
                .MakeGenericMethod(marker);
        Type delegateType = typeof(CollectibleGenericRouteOperation<>)
            .MakeGenericType(marker);
        Delegate original = originalMethod.CreateDelegate(delegateType);
        MethodInfo binder = typeof(MockInterception)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(static method =>
                method.Name ==
                    nameof(MockInterception.BindOwnedInstanceCaller) &&
                method.IsGenericMethodDefinition);
        var wrapper = (Delegate)binder.MakeGenericMethod(delegateType)
            .Invoke(
                null,
                [originalMethod, 0, operation, original])!;
        object target = Activator.CreateInstance(targetType)!;

        Assert.AreEqual(8, wrapper.DynamicInvoke(target, 7));

        context.Unload();
        return contextReference;
    }

    private static void CollectUntilDead(WeakReference reference)
    {
        for (var attempt = 0;
             attempt < 10 && reference.IsAlive;
             attempt++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
        }
    }
}

/// <summary>Provides one method owned by a copied collectible test assembly.</summary>
public static class CollectibleRouteMarker
{
    /// <summary>Provides the active delegate released when its route retires.</summary>
    public static void Wrapper()
    {
    }
}

/// <summary>Provides a non-collectible exact original route fallback.</summary>
internal static class CollectibleRouteFallback
{
    internal static int InvocationCount;

    /// <summary>Records one invocation of the retained original fallback.</summary>
    internal static void Invoke() =>
        InvocationCount++;
}

/// <summary>Provides a default-context generic operation closed over a collectible type.</summary>
public sealed class CollectibleGenericRouteTarget<T>
{
    /// <summary>Returns a deterministic original-operation result.</summary>
    public int Invoke(int value) =>
        value + 1;
}

/// <summary>Defines an exact route delegate closed over a collectible type.</summary>
public delegate int CollectibleGenericRouteOperation<T>(
    CollectibleGenericRouteTarget<T> target,
    int value);

/// <summary>Provides an exact original delegate for a constructed generic route.</summary>
public static class CollectibleGenericRouteOriginal
{
    /// <summary>Invokes the untouched constructed generic operation.</summary>
    public static int Invoke<T>(
        CollectibleGenericRouteTarget<T> target,
        int value) =>
        target.Invoke(value);
}
