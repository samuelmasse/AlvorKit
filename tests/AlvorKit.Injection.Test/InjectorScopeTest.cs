namespace AlvorKit.Injection.Test;

[TestClass]
public class InjectorScopeTest
{
    /// <summary>A valid scoped type creates a child scope with parent state and attribute metadata.</summary>
    [TestMethod]
    public void Scope_ValidType_CreatesNewScope()
    {
        var injector = new Injector();
        var scope = injector.Scope<ValidScope>();

        Assert.IsNotNull(scope.State);
        Assert.AreSame(injector.State, scope.State.Parent);
        Assert.AreEqual(typeof(ValidAttribute), scope.State.AttributeType);
    }

    /// <summary>Scope types missing their own required injector attribute are rejected.</summary>
    [TestMethod]
    public void Scope_UnScopedType_ThrowsException()
    {
        var injector = new Injector();
        Assert.ThrowsException<InjectorException>(injector.Scope<NonScopedScope>);
    }

    /// <summary>An uninitialized scope instance cannot create scopes or resolve services.</summary>
    [TestMethod]
    public void Scope_NotCreatedByInjector_ThrowsException()
    {
        var scope = new ValidScope();
        Assert.ThrowsException<InjectorScopeException>(scope.Scope<SubValidScope>);
        Assert.ThrowsException<InjectorScopeException>(scope.Get<ScopedService>);
        Assert.ThrowsException<InjectorScopeException>(scope.New<ScopedService>);
    }

    /// <summary>Scope types without a parameterless public constructor are rejected.</summary>
    [TestMethod]
    public void Scope_NoParameterlessConstructor_ThrowsException()
    {
        var injector = new Injector();
        Assert.ThrowsException<InjectorScopeException>(injector.Scope<NoParameterlessConstructorScope>);
    }

    /// <summary>Scope types without any public constructor are rejected.</summary>
    [TestMethod]
    public void Scope_NoPublicConstructor_ThrowsException()
    {
        var injector = new Injector();
        Assert.ThrowsException<InjectorScopeException>(injector.Scope<PrivateConstructorScope>);
    }

    /// <summary>Scope types using the non-generic scope base are rejected.</summary>
    [TestMethod]
    public void Scope_NonGenericInjectorScope_ThrowsException()
    {
        var injector = new Injector();
        Assert.ThrowsException<InjectorScopeException>(injector.Scope<NonGenericInjectorScope>);
    }

    /// <summary>A child scope cannot repeat an injector attribute type already present in its parent chain.</summary>
    [TestMethod]
    public void Scope_RepeatedAttribute_ThrowsException()
    {
        var injector = new Injector();
        var scope = injector.Scope<ValidScope>();
        Assert.ThrowsException<InjectorScopeException>(scope.Scope<ValidScope>);
    }

    /// <summary>Unscoped services cannot be resolved inside an attribute-gated child scope.</summary>
    [TestMethod]
    public void Scope_GetUnscopedInstanceInScope_ThrowsException()
    {
        var injector = new Injector();
        var scope = injector.Scope<ValidScope>();
        Assert.ThrowsException<InjectorException>(scope.Get<ServiceA>);
    }

    /// <summary>Services marked for another scope attribute cannot be resolved in the wrong scope.</summary>
    [TestMethod]
    public void Scope_GetWrongScopeInstanceInScope_ThrowsException()
    {
        var injector = new Injector();
        var scope = injector.Scope<ValidScope>();
        Assert.ThrowsException<InjectorException>(scope.Get<SubScopedService>);
    }

    /// <summary>Services with more than one injector attribute are rejected.</summary>
    [TestMethod]
    public void Scope_GetTooManyAttributes_ThrowsException()
    {
        var injector = new Injector();
        Assert.ThrowsException<InjectorException>(injector.Get<ServiceTooManyAttributes>);
    }

    /// <summary>A scoped service cannot be added to a parent scope that does not carry its attribute.</summary>
    [TestMethod]
    public void Scope_AddWrongScope_ThrowsException()
    {
        var injector = new Injector();
        var scope = injector.Scope<ValidScope>();
        var scopedService = scope.Get<ScopedService>();

        Assert.ThrowsException<InjectorException>(() => injector.Add(scopedService));
    }

    /// <summary>A scoped service cannot be created directly from a scope that does not carry its attribute.</summary>
    [TestMethod]
    public void Scope_NewWrongScope_ThrowsException()
    {
        var injector = new Injector();
        Assert.ThrowsException<InjectorException>(injector.New<ScopedService>);
    }

    /// <summary>Subscopes reuse parent scoped instances and root instances where appropriate.</summary>
    [TestMethod]
    public void Scope_SubScope_HasParentInstances()
    {
        var injector = new Injector();
        var scope = injector.Scope<ValidScope>();
        var subscope = scope.Scope<SubValidScope>();

        var subScopedService = subscope.Get<SubScopedService>();
        var scopedService = scope.Get<ScopedService>();
        var serviceE = injector.Get<ServiceE>();

        Assert.AreSame(serviceE, subScopedService.ServiceE);
        Assert.AreSame(serviceE, scopedService.ServiceE);
        Assert.AreSame(scopedService, subScopedService.ScopedService);
    }

    /// <summary>A parent scope rejects services that depend on a child-scoped dependency.</summary>
    [TestMethod]
    public void Scope_SubScopeInjectWithDependencyOutOfScope_ThrowsException()
    {
        var injector = new Injector();
        var scope = injector.Scope<ValidScope>();
        var subscope = scope.Scope<SubValidScope>();

        Assert.ThrowsException<InjectorException>(scope.Get<InvalidScopedService>);
    }

    /// <summary>Include patterns configured on a parent scope apply to child scopes.</summary>
    [TestMethod]
    public void Scope_IncludeDefinedByParent_IsUsed()
    {
        var injector = new Injector();
        var scope = injector.Scope<ValidScope>();
        injector.Include(new(@"^Bob\."));

        Assert.ThrowsException<InjectorException>(scope.Get<ScopedService>);
    }

    /// <summary>Custom handlers configured on a parent scope are available to child scopes.</summary>
    [TestMethod]
    public void Scope_CustomHandlerDefinedByParent_IsUsed()
    {
        var injector = new Injector();
        injector.Handler(new ScopedCustomHandler());
        var scope = injector.Scope<ValidScope>();
        scope.Handler(new CustomHandler());

        var custom = scope.Get<ScopedCustomService>();

        Assert.AreEqual("Injected Scoped", custom.Message);
    }

    /// <summary>Separate scope branches may use the same attribute types without sharing branch-local services.</summary>
    [TestMethod]
    public void Scope_BranchingSameScopeAttribute_IsAllowed()
    {
        var injector = new Injector();

        var scope1 = injector.Scope<ValidScope>();
        var scope2 = injector.Scope<ValidScope>();

        var subscope11 = scope1.Scope<SubValidScope>();
        var subscope12 = scope1.Scope<SubValidScope>();

        var subscope21 = scope2.Scope<SubValidScope>();
        var subscope22 = scope2.Scope<SubValidScope>();

        var subscopeService11 = subscope11.Get<SubScopedService>();
        var subscopeService12 = subscope12.Get<SubScopedService>();

        var subscopeService21 = subscope21.Get<SubScopedService>();
        var subscopeService22 = subscope22.Get<SubScopedService>();

        var scopeService1 = scope1.Get<ScopedService>();
        var scopeService2 = scope2.Get<ScopedService>();

        var serviceE = injector.Get<ServiceE>();

        Assert.AreSame(subscopeService11.ScopedService, subscopeService12.ScopedService);
        Assert.AreSame(subscopeService12.ScopedService, scopeService1);

        Assert.AreSame(subscopeService21.ScopedService, subscopeService22.ScopedService);
        Assert.AreSame(subscopeService22.ScopedService, scopeService2);

        Assert.AreSame(subscopeService11.ServiceE, subscopeService12.ServiceE);
        Assert.AreSame(subscopeService12.ServiceE, subscopeService21.ServiceE);
        Assert.AreSame(subscopeService21.ServiceE, subscopeService22.ServiceE);
        Assert.AreSame(subscopeService22.ServiceE, scopeService1.ServiceE);
        Assert.AreSame(scopeService1.ServiceE, scopeService2.ServiceE);
        Assert.AreSame(scopeService2.ServiceE, serviceE);
    }

    /// <summary>Run receives the concrete scope type and returns the original scope instance.</summary>
    [TestMethod]
    public void Run_InvokesActionWithConcreteScopeAndReturnsScope()
    {
        var injector = new Injector();
        var scope = injector.Scope<ValidScope>();
        ValidScope? actionScope = null;

        var returned = scope.Run(x => actionScope = x);

        Assert.AreSame(scope, actionScope);
        Assert.AreSame(scope, returned);
    }

    /// <summary>With adds an existing instance to the current scope and returns the original scope instance.</summary>
    [TestMethod]
    public void With_Instance_AddsInstanceAndReturnsScope()
    {
        var injector = new Injector();
        var scope = injector.Scope<ValidScope>();
        var service = new ScopedService(injector.Get<ServiceE>());

        var returned = scope.With(service);

        Assert.AreSame(scope, returned);
        Assert.AreSame(service, scope.Get<ScopedService>());
    }

    /// <summary>With creates an instance from the concrete scope, adds it to that scope, and returns the original scope instance.</summary>
    [TestMethod]
    public void With_Factory_AddsCreatedInstanceAndReturnsScope()
    {
        var injector = new Injector();
        var scope = injector.Scope<ValidScope>();

        var returned = scope.With(x => new SeededScopedService(x.Get<ScopedService>()));

        Assert.AreSame(scope, returned);
        Assert.AreSame(scope.Get<ScopedService>(), scope.Get<SeededScopedService>().ScopedService);
    }
}
