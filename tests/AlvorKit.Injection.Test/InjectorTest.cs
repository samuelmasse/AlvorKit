namespace AlvorKit.Injection.Test;

[TestClass]
public class InjectorTest
{
    /// <summary>A root injector creates an uncached concrete dependency on first typed request.</summary>
    [TestMethod]
    public void Injector_GetInstance_Works()
    {
        var injector = new Injector();

        var instance = injector.Get<ServiceA>();

        Assert.IsNotNull(instance);
        Assert.IsInstanceOfType(instance, typeof(ServiceA));
    }

    /// <summary>A root injector creates an uncached concrete dependency on first opaque type request.</summary>
    [TestMethod]
    public void Injector_GetInstanceOpaque_Works()
    {
        var injector = new Injector();

        var instance = injector.Get(typeof(ServiceA));

        Assert.IsNotNull(instance);
        Assert.IsInstanceOfType(instance, typeof(ServiceA));
    }

    /// <summary>Constructor dependencies are resolved recursively for typed requests.</summary>
    [TestMethod]
    public void Injector_Get_ResolvesDependencies()
    {
        var injector = new Injector();

        var instance = injector.Get<ServiceB>();

        Assert.IsNotNull(instance);
        Assert.IsInstanceOfType(instance, typeof(ServiceB));
        Assert.IsNotNull(instance.ServiceA);
    }

    /// <summary>Constructor dependencies are resolved recursively across a larger service graph.</summary>
    [TestMethod]
    public void Injector_Get_ResolvesDependenciesComplex()
    {
        var injector = new Injector();

        var instance = injector.Get<ServiceE>();

        Assert.IsNotNull(instance);
        Assert.IsInstanceOfType(instance, typeof(ServiceE));
        Assert.IsNotNull(instance.ServiceA);
        Assert.IsNotNull(instance.ServiceB);
        Assert.IsNotNull(instance.ServiceC);
        Assert.IsNotNull(instance.ServiceD);

        Assert.IsNotNull(instance.ServiceB.ServiceA);
        Assert.IsNotNull(instance.ServiceC.ServiceA);
        Assert.IsNotNull(instance.ServiceC.ServiceB);
        Assert.IsNotNull(instance.ServiceD.ServiceA);
        Assert.IsNotNull(instance.ServiceD.ServiceB);
        Assert.IsNotNull(instance.ServiceD.ServiceC);
    }

    /// <summary>Circular constructor dependencies are rejected with an injector exception.</summary>
    [TestMethod]
    public void Injector_GetCircularDependency_ThrowsException()
    {
        var injector = new Injector();
        Assert.ThrowsException<InjectorException>(injector.Get<ServiceCircularA>);
    }

    /// <summary>A constructor that re-enters resolution for its own type reports a circular dependency.</summary>
    [TestMethod]
    public void Injector_GetCircularDependencyInConstructor_ThrowsException()
    {
        var injector = new Injector();
        Assert.ThrowsException<InjectorException>(injector.Get<ServiceCircularConstructor>);
    }

    /// <summary>A service can resolve other services from the injector passed into its constructor.</summary>
    [TestMethod]
    public void Injector_GetInConstructor_HasCorrectInstance()
    {
        var injector = new Injector();

        var service = injector.Get<ServiceUseInjectorInConstructor>();
        var serviceE = injector.Get<ServiceE>();

        Assert.AreSame(service.ServiceD, serviceE.ServiceD);
    }

    /// <summary>Exceptions thrown by constructors are wrapped with injector path diagnostics.</summary>
    [TestMethod]
    public void Injector_GetConstructorThatThrows_IsHandled()
    {
        var injector = new Injector();
        Assert.ThrowsException<InjectorException>(injector.Get<ServiceThrowInConstructor>);
    }

    /// <summary>Services with multiple public constructors are rejected.</summary>
    [TestMethod]
    public void Injector_GetInvalidConstructors_ThrowsException()
    {
        var injector = new Injector();
        Assert.ThrowsException<InjectorException>(injector.Get<ServiceBad>);
    }

    /// <summary>Services without accessible public constructors are rejected.</summary>
    [TestMethod]
    public void Injector_GetNoConstructor_ThrowsException()
    {
        var injector = new Injector();
        Assert.ThrowsException<InjectorException>(injector.Get<ServiceNoConstructor>);
    }

    /// <summary>Include filters reject dependencies outside the allowed namespace pattern.</summary>
    [TestMethod]
    public void Injector_GetWithExcludedDependencies_ThrowsException()
    {
        var injector = new Injector();
        injector.Include(new(@"^AlvorKit\.Injection\."));

        Assert.ThrowsException<InjectorException>(injector.Get<ServiceInvalidDependencies>);
    }

    /// <summary>Include filters reject open generic base types that do not match the allowed pattern.</summary>
    [TestMethod]
    public void Injector_GetWithExcludedDependenciesWeirdType_ThrowsException()
    {
        var injector = new Injector();
        injector.Include(new(@"^AlvorKit\.Injection\."));

        Type t = typeof(Derived<>);
        Type baseType = t.BaseType!;

        Assert.ThrowsException<InjectorException>(() => injector.Get(baseType));
    }

    /// <summary>Include filters allow dependencies inside the permitted namespace pattern.</summary>
    [TestMethod]
    public void Injector_GetWithIncludedDependencies_InjectsProperly()
    {
        var injector = new Injector();
        injector.Include(new(@"^AlvorKit\.Injection\."));

        injector.Get<ServiceE>();
    }

    /// <summary>Typed new requests create distinct root service instances.</summary>
    [TestMethod]
    public void Injector_New_CreatesNewInstance()
    {
        var injector = new Injector();

        var instance1 = injector.New<ServiceE>();
        var instance2 = injector.New<ServiceE>();

        Assert.AreNotSame(instance1, instance2);
    }

    /// <summary>Opaque type new requests create distinct root service instances.</summary>
    [TestMethod]
    public void Injector_NewOpaque_CreatesNewInstance()
    {
        var injector = new Injector();

        var instance1 = injector.New(typeof(ServiceE));
        var instance2 = injector.New(typeof(ServiceE));

        Assert.AreNotSame(instance1, instance2);
    }

    /// <summary>Typed get requests return the same cached root service instance.</summary>
    [TestMethod]
    public void Injector_Get_ReturnsSameInstance()
    {
        var injector = new Injector();

        var instance1 = injector.Get<ServiceE>();
        var instance2 = injector.Get<ServiceE>();

        Assert.AreSame(instance1, instance2);
    }

    /// <summary>Explicitly added instances are returned by later get requests for their type.</summary>
    [TestMethod]
    public void Injector_Add_ReturnsSameInstance()
    {
        var injector = new Injector();
        var serviceA = new ServiceA();

        injector.Add(serviceA);

        Assert.AreSame(serviceA, injector.Get<ServiceA>());
    }

    /// <summary>Adding the same instance twice to a scope is rejected.</summary>
    [TestMethod]
    public void Injector_DoubleAddSame_ThrowsException()
    {
        var injector = new Injector();
        var serviceA = new ServiceA();
        injector.Add(serviceA);
        Assert.ThrowsException<InjectorException>(() => injector.Add(serviceA));
    }

    /// <summary>Adding a different instance of an already registered type is rejected.</summary>
    [TestMethod]
    public void Injector_DoubleAddDifferent_ThrowsException()
    {
        var injector = new Injector();
        var serviceA = new ServiceA();
        var serviceA2 = new ServiceA();
        injector.Add(serviceA);
        Assert.ThrowsException<InjectorException>(() => injector.Add(serviceA2));
    }

    /// <summary>Custom handlers instantiate matching service types before the default handler is used.</summary>
    [TestMethod]
    public void Injector_CustomHandler_Works()
    {
        var injector = new Injector();
        injector.Handler(new CustomHandler());
        injector.Handler(new SecondCustomHandler());

        var custom = injector.Get<CustomService>();
        var secondCustom = injector.Get<SecondCustomService>();

        Assert.AreEqual("Injected", custom.Message);
        Assert.AreEqual("Injected Also", secondCustom.Message);
    }

    /// <summary>Exceptions thrown by a custom handler during instantiation are wrapped.</summary>
    [TestMethod]
    public void Injector_CustomHandlerThatThrows_IsHandled()
    {
        var injector = new Injector();
        injector.Handler(new CustomHandlerThrows());

        Assert.ThrowsException<InjectorException>(injector.Get<CustomService>);
    }

    /// <summary>Exceptions thrown while a custom handler checks a type are wrapped.</summary>
    [TestMethod]
    public void Injector_CustomHandlerThatThrowsOnHandles_IsHandled()
    {
        var injector = new Injector();
        injector.Handler(new CustomHandlerHandlesThrows());

        Assert.ThrowsException<InjectorException>(injector.Get<CustomService>);
    }

    /// <summary>Custom handlers that return the wrong concrete type are rejected.</summary>
    [TestMethod]
    public void Injector_CustomHandlerWrongType_IsHandled()
    {
        var injector = new Injector();
        injector.Handler(new CustomHandlerWrongType());

        Assert.ThrowsException<InjectorException>(injector.Get<CustomService>);
    }

    /// <summary>The first matching custom handler wins when a later handler would throw.</summary>
    [TestMethod]
    public void Injector_CustomHandlerWithSameType_RespectsOrderAndDoesNotThrow()
    {
        var injector = new Injector();
        injector.Handler(new CustomHandler());
        injector.Handler(new CustomHandlerThrows());

        var custom = injector.Get<CustomService>();
        Assert.AreEqual("Injected", custom.Message);
    }

    /// <summary>The first matching custom handler wins when it throws before a later valid handler.</summary>
    [TestMethod]
    public void Injector_CustomHandlerWithSameType_RespectsOrderAndThrows()
    {
        var injector2 = new Injector();
        injector2.Handler(new CustomHandlerThrows());
        injector2.Handler(new CustomHandler());

        Assert.ThrowsException<InjectorException>(injector2.Get<CustomService>);
    }

    /// <summary>Struct services and services depending on them reuse cached dependency instances.</summary>
    [TestMethod]
    public void Injector_GetStruct_ReturnsSameStructs()
    {
        var injector = new Injector();

        var serviceA = injector.Get<ServiceA>();
        var serviceB = injector.Get<ServiceB>();
        var structService = injector.Get<StructService>();
        var structServiceUser = injector.Get<StructServiceUser>();

        Assert.IsNotNull(structService.Rng);
        Assert.AreSame(serviceA, structService.ServiceA);
        Assert.AreSame(serviceB, structService.ServiceB);
        Assert.AreSame(structService.Rng, structServiceUser.StructService.Rng);
    }
}
