namespace AlvorKit;

[TestClass]
public class InjectorSuppliedInstanceTest
{
    /// <summary>Every Add overload publishes supplied unmarked instances to local and descendant constructors.</summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    public void Add_UnmarkedInstance_UsesRegisteringScope(int registration)
    {
        var injector = new Injector();
        var parent = injector.Scope<BindingScope>();
        var child = parent.Scope<OtherBindingScope>();
        var observer = new SuppliedInstanceObserver();
        injector.Observe(observer);
        var instance = new SuppliedGenerator();

        Add(parent, instance, registration);

        Assert.AreSame(instance, parent.Get<SuppliedGenerator>());
        Assert.AreSame(instance, parent.Get<SuppliedGeneratorConsumer>().Generator);
        Assert.AreSame(instance, child.Get<SuppliedGenerator>());
        Assert.AreSame(instance, child.Get<SuppliedGeneratorChildConsumer>().Generator);
        Assert.AreNotSame(instance, injector.Get<SuppliedGenerator>());

        if (registration >= 3)
        {
            Assert.AreSame(instance, child.Get<ISharedGenerator>());
            Assert.AreSame(instance, child.Get<SuppliedGeneratorAliasConsumer>().Generator);
        }

        var ownership = observer.Instances.Where(x => ReferenceEquals(x.Instance, instance)).ToArray();
        Assert.HasCount(1, ownership);
        Assert.AreSame(parent, ownership[0].Owner);
        Assert.ThrowsException<InjectorException>(parent.New<SuppliedGenerator>);
        Assert.ThrowsException<InjectorException>(child.New<SuppliedGenerator>);
    }

    /// <summary>Sibling scopes own separate supplied instances even when the root has already cached that type.</summary>
    [TestMethod]
    public void Add_UnmarkedInstance_IsolatesSiblingScopes()
    {
        var injector = new Injector();
        var rootInstance = injector.Get<SuppliedGenerator>();
        var first = injector.Scope<BindingScope>();
        var second = injector.Scope<BindingScope>();
        var firstInstance = new SuppliedGenerator();
        var secondInstance = new SuppliedGenerator();
        first.Add(firstInstance);
        second.With(secondInstance);

        Assert.AreSame(firstInstance, first.Get<SuppliedGeneratorConsumer>().Generator);
        Assert.AreSame(secondInstance, second.Get<SuppliedGeneratorConsumer>().Generator);
        Assert.AreSame(rootInstance, injector.Get<SuppliedGenerator>());
    }

    /// <summary>A nearer supplied instance shadows an ancestor registration without replacing the ancestor's object.</summary>
    [TestMethod]
    public void Add_UnmarkedInstance_ChildOverridesParent()
    {
        var parent = new Injector().Scope<BindingScope>();
        var parentInstance = new SuppliedGenerator();
        parent.Add(parentInstance);
        var child = parent.Scope<OtherBindingScope>();
        var childInstance = new SuppliedGenerator();
        child.Add(childInstance);

        Assert.AreSame(childInstance, child.Get<SuppliedGeneratorChildConsumer>().Generator);
        Assert.AreSame(parentInstance, parent.Get<SuppliedGeneratorConsumer>().Generator);
    }

    /// <summary>Additional aliases reuse the supplied concrete object and retain rejection of duplicate aliases.</summary>
    [TestMethod]
    public void Add_UnmarkedInstance_AdditionalAliasSharesConcreteInstance()
    {
        var parent = new Injector().Scope<BindingScope>();
        var instance = new SuppliedGenerator();
        parent.Add(instance);
        parent.Add<ISharedGenerator>(instance);
        var child = parent.Scope<OtherBindingScope>();

        Assert.AreSame(instance, child.Get<SuppliedGenerator>());
        Assert.AreSame(instance, child.Get<ISharedGenerator>());
        Assert.ThrowsException<InjectorException>(() => parent.Add<ISharedGenerator>(instance));
        Assert.ThrowsException<InjectorException>(child.New<ISharedGenerator>);
    }

    /// <summary>Supplied neutral instances do not change the construction scope of their ordinary dependencies.</summary>
    [TestMethod]
    public void Add_UnmarkedInstance_DoesNotPublishItsObjectGraph()
    {
        var injector = new Injector();
        var parent = injector.Scope<BindingScope>();
        var nested = new ServiceA();
        parent.Add(new SuppliedContainer(nested));

        Assert.AreSame(nested, parent.Get<SuppliedContainer>().Service);
        Assert.AreNotSame(nested, parent.Get<ScopedServiceAConsumer>().Service);
        Assert.AreSame(injector.Get<ServiceA>(), parent.Get<ScopedServiceAConsumer>().Service);
        Assert.ThrowsException<InjectorException>(parent.Get<ServiceA>);
        Assert.ThrowsException<InjectorException>(parent.Bind<ServiceA>);
        Assert.ThrowsException<InjectorException>(() => parent.Bind(new ServiceA()));
    }

    /// <summary>All Add overloads retain duplicate concrete-instance checks for newly supported neutral instances.</summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    public void Add_UnmarkedInstance_RejectsDuplicates(int registration)
    {
        var scope = new Injector().Scope<BindingScope>();
        var instance = new SuppliedGenerator();
        Add(scope, instance, registration);

        Assert.ThrowsException<InjectorException>(() => Add(scope, instance, registration));
        Assert.ThrowsException<InjectorException>(() => Add(scope, new SuppliedGenerator(), registration));
        Assert.ThrowsException<InjectorException>(() => scope.Add((object)new SuppliedGenerator()));
    }

    /// <summary>Neutral-instance support does not relax marked provider or marked service constraints.</summary>
    [TestMethod]
    public void Add_PreservesAttributeConstraints()
    {
        var injector = new Injector();
        var scope = injector.Scope<BindingScope>();

        Assert.ThrowsException<InjectorException>(() => scope.Add((object)new OtherScopedGenerator()));
        Assert.ThrowsException<InjectorException>(() => scope.Add(new OtherScopedGenerator()));
        Assert.ThrowsException<InjectorException>(() => scope.Add<IUnmarkedGenerator>(new OtherScopedGenerator()));
        Assert.ThrowsException<InjectorException>(() => scope.Add<IBindingBiomeGenerator>(new UnmarkedBiomeGenerator()));
        Assert.ThrowsException<InjectorException>(() => injector.Add((object)new UnrelatedBindingService()));
    }

    /// <summary>Include filters still apply to the runtime type when supplying an unmarked instance.</summary>
    [TestMethod]
    [DataRow(0)]
    [DataRow(1)]
    [DataRow(2)]
    [DataRow(3)]
    [DataRow(4)]
    public void Add_UnmarkedInstance_PreservesIncludeFilters(int registration)
    {
        var scope = new Injector().Scope<BindingScope>();
        scope.Include(new("^ExcludedNamespace\\."));

        Assert.ThrowsException<InjectorException>(() => Add(scope, new SuppliedGenerator(), registration));
    }

    /// <summary>Previously allowed object registration retains uncached New construction where attributes allow it.</summary>
    [TestMethod]
    public void New_AfterObjectRegistration_PreservesExistingConstruction()
    {
        var injector = new Injector();
        var rootInstance = new ServiceA();
        injector.Add((object)rootInstance);
        var scope = injector.Scope<BindingScope>();
        var scopedInstance = new UnrelatedBindingService();
        scope.Add((object)scopedInstance);

        Assert.AreNotSame(rootInstance, injector.New<ServiceA>());
        Assert.AreSame(rootInstance, injector.Get<ServiceA>());
        Assert.AreNotSame(scopedInstance, scope.New<UnrelatedBindingService>());
        Assert.AreSame(scopedInstance, scope.Get<UnrelatedBindingService>());
    }

    private static void Add(InjectorScope scope, SuppliedGenerator instance, int registration)
    {
        switch (registration)
        {
            case 0: scope.Add((object)instance); break;
            case 1: scope.Add(instance); break;
            case 2: scope.Add(typeof(SuppliedGenerator), instance); break;
            case 3: scope.Add<ISharedGenerator>(instance); break;
            case 4: scope.Add(typeof(ISharedGenerator), instance); break;
            default: throw new ArgumentOutOfRangeException(nameof(registration));
        }
    }
}
