namespace AlvorKit.Injection.Test;

[TestClass]
public class InjectorConcurrencyTest
{
    /// <summary>Concurrent get requests for the same type all observe the same cached instance.</summary>
    [TestMethod]
    public void Injector_GetConcurrent_AreAllSameInstance()
    {
        var injector = new Injector();

        var result = new ServiceE[10000];

        Parallel.For(0, 10000, (i) =>
        {
            result[i] = injector.Get<ServiceE>();
        });

        for (int i = 1; i < result.Length; i++)
            Assert.AreSame(result[i], result[i - 1]);
    }

    /// <summary>Concurrent new requests create distinct top-level instances while reusing cached dependencies.</summary>
    [TestMethod]
    public void Injector_NewConcurrent_AreAllSameDependencies()
    {
        var injector = new Injector();

        var result = new ServiceE[1000];

        Parallel.For(0, 1000, (i) =>
        {
            result[i] = injector.New<ServiceE>();
        });

        for (int i = 0; i < result.Length; i++)
            Assert.AreSame(injector.Get<ServiceA>(), result[i].ServiceA);
    }

    /// <summary>Concurrent branch and subscope creation completes without corrupting shared root state.</summary>
    [TestMethod]
    public void Scope_Concurrent_Works()
    {
        var injector = new Injector();

        Parallel.For(0, 100, (i) =>
        {
            var scope = injector.Scope<ValidScope>();

            Parallel.For(0, 100, (j) => scope.Scope<SubValidScope>());
        });
    }

    /// <summary>Concurrent subscopes reuse root instances and their own parent scope instances correctly.</summary>
    [TestMethod]
    public void Scope_GetConcurrent_AreAllSameParentInstances()
    {
        var injector = new Injector();
        var bag = new ConcurrentBag<(ValidScope, SubScopedService)>();

        Parallel.For(0, 100, (i) =>
        {
            var scope = injector.Scope<ValidScope>();

            Parallel.For(0, 100, (j) =>
            {
                var subscope = scope.Scope<SubValidScope>();
                bag.Add((scope, subscope.Get<SubScopedService>()));
            });
        });

        while (!bag.IsEmpty)
        {
            bag.TryTake(out var res);
            var (scope, subScopedService) = res;

            Assert.AreSame(injector.Get<ServiceA>(), subScopedService.ServiceE.ServiceA);
            Assert.AreSame(scope.Get<ScopedService>(), subScopedService.ScopedService);
        }
    }
}
