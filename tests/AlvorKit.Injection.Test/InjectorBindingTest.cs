namespace AlvorKit.Injection.Test;

[TestClass]
public class InjectorBindingTest
{
    /// <summary>A single implementation bind resolves a marked interface service.</summary>
    [TestMethod]
    public void Bind_Interface_ResolvesImplementation()
    {
        var scope = new Injector().Scope<BindingScope>();
        scope.Bind<BindingBiomeGenerator>();

        var generator = scope.Get<IBindingBiomeGenerator>();

        Assert.IsInstanceOfType<BindingBiomeGenerator>(generator);
    }

    /// <summary>A single implementation bind resolves a marked abstract base service.</summary>
    [TestMethod]
    public void Bind_BaseClass_ResolvesImplementation()
    {
        var scope = new Injector().Scope<BindingScope>();
        scope.Bind<BindingBiomeGenerator>();

        var generator = scope.Get<BindingBiomeGeneratorBase>();

        Assert.IsInstanceOfType<BindingBiomeGenerator>(generator);
    }

    /// <summary>Interface, base, and concrete requests share one implementation instance.</summary>
    [TestMethod]
    public void Bind_ServiceSurfaces_ShareInstance()
    {
        var scope = new Injector().Scope<BindingScope>();
        scope.Bind<BindingBiomeGenerator>();

        var asInterface = scope.Get<IBindingBiomeGenerator>();
        var asBase = scope.Get<BindingBiomeGeneratorBase>();
        var asConcrete = scope.Get<BindingBiomeGenerator>();

        Assert.AreSame((object)asInterface, asBase);
        Assert.AreSame((object)asBase, asConcrete);
    }

    /// <summary>Binding after concrete resolution aliases the existing concrete instance.</summary>
    [TestMethod]
    public void Bind_AfterConcreteResolution_SharesExistingInstance()
    {
        var scope = new Injector().Scope<BindingScope>();
        var concrete = scope.Get<BindingBiomeGenerator>();

        scope.Bind<BindingBiomeGenerator>();

        Assert.AreSame(concrete, scope.Get<IBindingBiomeGenerator>());
        Assert.AreSame(concrete, scope.Get<BindingBiomeGeneratorBase>());
    }

    /// <summary>An explicit unmarked interface binding resolves the implementation.</summary>
    [TestMethod]
    public void Bind_UnmarkedInterface_ResolvesImplementation()
    {
        var scope = new Injector().Scope<BindingScope>();
        scope.Bind<ISharedGenerator, ParentSharedGenerator>();

        var generator = scope.Get<ISharedGenerator>();

        Assert.IsInstanceOfType<ParentSharedGenerator>(generator);
    }

    /// <summary>The non-generic implementation bind resolves marked service surfaces.</summary>
    [TestMethod]
    public void Bind_NonGenericImplementation_ResolvesImplementation()
    {
        var scope = new Injector().Scope<BindingScope>();
        scope.Bind(typeof(BindingBiomeGenerator));

        var generator = scope.Get<IBindingBiomeGenerator>();

        Assert.IsInstanceOfType<BindingBiomeGenerator>(generator);
    }

    /// <summary>The non-generic explicit bind resolves the requested service surface.</summary>
    [TestMethod]
    public void Bind_NonGenericExplicitService_ResolvesImplementation()
    {
        var scope = new Injector().Scope<BindingScope>();
        scope.Bind(typeof(IBindingBiomeGenerator), typeof(BindingBiomeGenerator));

        var generator = scope.Get<IBindingBiomeGenerator>();

        Assert.IsInstanceOfType<BindingBiomeGenerator>(generator);
    }

    /// <summary>A child scope can override a parent binding for an unmarked shared service.</summary>
    [TestMethod]
    public void Bind_ChildBindingOverridesParentBinding()
    {
        var parent = new Injector().Scope<BindingScope>();
        parent.Bind<ISharedGenerator, ParentSharedGenerator>();
        var child = parent.Scope<OtherBindingScope>();
        child.Bind<ISharedGenerator, ChildSharedGenerator>();

        var generator = child.Get<ISharedGenerator>();

        Assert.AreEqual("child", generator.Name);
    }

    /// <summary>A child scope uses a parent binding when it has no nearer binding.</summary>
    [TestMethod]
    public void Bind_ParentBindingVisibleToChild()
    {
        var parent = new Injector().Scope<BindingScope>();
        parent.Bind<ISharedGenerator, ParentSharedGenerator>();
        var child = parent.Scope<OtherBindingScope>();

        var generator = child.Get<ISharedGenerator>();

        Assert.AreEqual("parent", generator.Name);
    }

    /// <summary>Implementation bindings reject implementation types that do not satisfy the service type.</summary>
    [TestMethod]
    public void Bind_WrongImplementationType_ThrowsException()
    {
        var scope = new Injector().Scope<BindingScope>();

        Assert.ThrowsException<InjectorException>(() =>
            scope.Bind(typeof(IBindingBiomeGenerator), typeof(UnrelatedBindingService)));
    }

    /// <summary>Explicit implementation bindings reject binding a type to itself.</summary>
    [TestMethod]
    public void Bind_SelfBinding_ThrowsException()
    {
        var scope = new Injector().Scope<BindingScope>();

        Assert.ThrowsException<InjectorException>(() =>
            scope.Bind<BindingBiomeGenerator, BindingBiomeGenerator>());
    }

    /// <summary>Implementation-only bindings reject open generic implementation types.</summary>
    [TestMethod]
    public void Bind_OpenGenericImplementation_ThrowsException()
    {
        var scope = new Injector().Scope<BindingScope>();

        Assert.ThrowsException<InjectorException>(() =>
            scope.Bind(typeof(OpenBindingService<>)));
    }

    /// <summary>Implementation-only bindings reject interface implementation types.</summary>
    [TestMethod]
    public void Bind_InterfaceImplementation_ThrowsException()
    {
        var scope = new Injector().Scope<BindingScope>();

        Assert.ThrowsException<InjectorException>(() =>
            scope.Bind(typeof(IBindingBiomeGenerator)));
    }

    /// <summary>Implementation bindings reject concrete types from the wrong scope.</summary>
    [TestMethod]
    public void Bind_WrongScopeImplementation_ThrowsException()
    {
        var scope = new Injector().Scope<BindingScope>();

        Assert.ThrowsException<InjectorException>(() =>
            scope.Bind<IUnmarkedGenerator, OtherScopedGenerator>());
    }

    /// <summary>Implementation bindings reject service attributes that conflict with provider attributes.</summary>
    [TestMethod]
    public void Bind_ServiceAttributeConflict_ThrowsException()
    {
        var scope = new Injector().Scope<BindingScope>();

        Assert.ThrowsException<InjectorException>(() =>
            scope.Bind<IOtherBindingGenerator, BindingWrongSurfaceGenerator>());
    }

    /// <summary>A scope cannot bind the same service key twice.</summary>
    [TestMethod]
    public void Bind_DuplicateServiceType_ThrowsException()
    {
        var scope = new Injector().Scope<BindingScope>();
        scope.Bind<IBindingBiomeGenerator, BindingBiomeGenerator>();

        Assert.ThrowsException<InjectorException>(() =>
            scope.Bind<IBindingBiomeGenerator, AlternateBindingBiomeGenerator>());
    }

    /// <summary>Instance binding exposes all compatible marked service surfaces.</summary>
    [TestMethod]
    public void Bind_Instance_ResolvesServiceSurfaces()
    {
        var injector = new Injector();
        var scope = injector.Scope<BindingScope>();
        var generator = new BindingBiomeGenerator(injector.Get<ServiceA>());
        scope.Bind(generator);

        Assert.AreSame(generator, scope.Get<IBindingBiomeGenerator>());
        Assert.AreSame(generator, scope.Get<BindingBiomeGeneratorBase>());
        Assert.AreSame(generator, scope.Get<BindingBiomeGenerator>());
    }

    /// <summary>A child scope uses a parent instance binding when it has no nearer binding.</summary>
    [TestMethod]
    public void Bind_InstanceParentBindingVisibleToChild()
    {
        var injector = new Injector();
        var parent = injector.Scope<BindingScope>();
        var generator = new BindingBiomeGenerator(injector.Get<ServiceA>());
        parent.Bind(generator);
        var child = parent.Scope<OtherBindingScope>();

        Assert.AreSame(generator, child.Get<IBindingBiomeGenerator>());
    }

    /// <summary>Instance binding rejects a second concrete instance for the same implementation type.</summary>
    [TestMethod]
    public void Bind_InstanceConcreteConflict_ThrowsException()
    {
        var injector = new Injector();
        var scope = injector.Scope<BindingScope>();
        scope.Bind(new BindingBiomeGenerator(injector.Get<ServiceA>()));

        Assert.ThrowsException<InjectorException>(() =>
            scope.Bind(new BindingBiomeGenerator(injector.Get<ServiceA>())));
    }

    /// <summary>Explicit service instance registration resolves the requested interface.</summary>
    [TestMethod]
    public void Add_ServiceInstance_ResolvesInterface()
    {
        var injector = new Injector();
        var scope = injector.Scope<BindingScope>();
        var generator = new BindingBiomeGenerator(injector.Get<ServiceA>());

        scope.Add<IBindingBiomeGenerator>(generator);

        Assert.AreSame(generator, scope.Get<IBindingBiomeGenerator>());
    }

    /// <summary>Non-generic service instance registration resolves the requested interface.</summary>
    [TestMethod]
    public void Add_NonGenericServiceInstance_ResolvesInterface()
    {
        var injector = new Injector();
        var scope = injector.Scope<BindingScope>();
        var generator = new BindingBiomeGenerator(injector.Get<ServiceA>());

        scope.Add(typeof(IBindingBiomeGenerator), generator);

        Assert.AreSame(generator, scope.Get<IBindingBiomeGenerator>());
    }

    /// <summary>Explicit service instance registration shares the supplied concrete instance.</summary>
    [TestMethod]
    public void Add_ServiceInstance_ConcreteRequestSharesInstance()
    {
        var injector = new Injector();
        var scope = injector.Scope<BindingScope>();
        var generator = new BindingBiomeGenerator(injector.Get<ServiceA>());

        scope.Add<IBindingBiomeGenerator>(generator);

        Assert.AreSame(generator, scope.Get<BindingBiomeGenerator>());
    }

    /// <summary>Service instance registration rejects objects that do not satisfy the service type.</summary>
    [TestMethod]
    public void Add_ServiceInstanceWrongType_ThrowsException()
    {
        var scope = new Injector().Scope<BindingScope>();

        Assert.ThrowsException<InjectorException>(() =>
            scope.Add(typeof(IBindingBiomeGenerator), new UnrelatedBindingService()));
    }

    /// <summary>Service instance registration rejects open generic service aliases.</summary>
    [TestMethod]
    public void Add_OpenGenericServiceType_ThrowsException()
    {
        var injector = new Injector();
        var scope = injector.Scope<BindingScope>();
        var generator = new BindingBiomeGenerator(injector.Get<ServiceA>());

        Assert.ThrowsException<InjectorException>(() =>
            scope.Add(typeof(OpenBindingService<>), generator));
    }

    /// <summary>Service instance registration rejects concrete instances from the wrong scope.</summary>
    [TestMethod]
    public void Add_ServiceInstanceWrongScope_ThrowsException()
    {
        var scope = new Injector().Scope<BindingScope>();

        Assert.ThrowsException<InjectorException>(() =>
            scope.Add<IUnmarkedGenerator>(new OtherScopedGenerator()));
    }

    /// <summary>Service instance registration rejects conflicting service and instance attributes.</summary>
    [TestMethod]
    public void Add_ServiceInstanceAttributeConflict_ThrowsException()
    {
        var scope = new Injector().Scope<BindingScope>();

        Assert.ThrowsException<InjectorException>(() =>
            scope.Add<IOtherBindingGenerator>(new BindingWrongSurfaceGenerator()));
    }

    /// <summary>A scope cannot add the same service alias twice.</summary>
    [TestMethod]
    public void Add_ServiceInstanceDuplicateServiceType_ThrowsException()
    {
        var injector = new Injector();
        var scope = injector.Scope<BindingScope>();
        var generator = new BindingBiomeGenerator(injector.Get<ServiceA>());
        scope.Add<IBindingBiomeGenerator>(generator);

        Assert.ThrowsException<InjectorException>(() =>
            scope.Add<IBindingBiomeGenerator>(generator));
    }

    /// <summary>New creates fresh implementations through an implementation binding.</summary>
    [TestMethod]
    public void New_InterfaceBinding_CreatesFreshImplementation()
    {
        var scope = new Injector().Scope<BindingScope>();
        scope.Bind<BindingBiomeGenerator>();

        var first = scope.New<IBindingBiomeGenerator>();
        var second = scope.New<IBindingBiomeGenerator>();

        Assert.AreNotSame(first, second);
    }

    /// <summary>New rejects service types bound only to an existing instance.</summary>
    [TestMethod]
    public void New_InstanceBinding_ThrowsException()
    {
        var injector = new Injector();
        var scope = injector.Scope<BindingScope>();
        scope.Bind(new BindingBiomeGenerator(injector.Get<ServiceA>()));

        Assert.ThrowsException<InjectorException>(scope.New<IBindingBiomeGenerator>);
    }

    /// <summary>Constructor interface parameters resolve from the binding owner scope.</summary>
    [TestMethod]
    public void Constructor_InterfaceDependency_UsesBindingScope()
    {
        var scope = new Injector().Scope<BindingScope>();
        scope.Bind<BindingBiomeGenerator>();

        var terrain = scope.Get<BindingTerrain>();

        Assert.AreSame(scope.Get<IBindingBiomeGenerator>(), terrain.Generator);
    }

    /// <summary>Constructor base-class parameters resolve from the binding owner scope.</summary>
    [TestMethod]
    public void Constructor_BaseDependency_UsesBindingScope()
    {
        var scope = new Injector().Scope<BindingScope>();
        scope.Bind<BindingBiomeGenerator>();

        var terrain = scope.Get<BindingBaseTerrain>();

        Assert.AreSame(scope.Get<BindingBiomeGeneratorBase>(), terrain.Generator);
    }

    /// <summary>Circular dependencies through service bindings are still rejected.</summary>
    [TestMethod]
    public void CircularDependencyThroughBinding_ThrowsException()
    {
        var scope = new Injector().Scope<BindingScope>();
        scope.Bind<CircularBindingService>();

        Assert.ThrowsException<InjectorException>(scope.Get<ICircularBindingService>);
    }

    /// <summary>Custom handlers can return a subtype assignable to the requested service.</summary>
    [TestMethod]
    public void CustomHandler_ReturnsAssignableSubtype_Works()
    {
        var injector = new Injector();
        injector.Handler(new InterfaceHandler());

        var service = injector.Get<IHandlerService>();

        Assert.AreEqual("handled", service.Message);
    }
}
