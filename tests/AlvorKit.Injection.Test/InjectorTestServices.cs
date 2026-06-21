namespace AlvorKit.Injection.Test;

public record class ServiceA;
public record class ServiceB(ServiceA ServiceA);
public record class ServiceC(ServiceA ServiceA, ServiceB ServiceB);
public record class ServiceD(ServiceB ServiceB, ServiceA ServiceA, ServiceC ServiceC);
public record class ServiceE(ServiceC ServiceC, ServiceD ServiceD, ServiceB ServiceB, ServiceA ServiceA);

public record class ServiceCircularA(ServiceCircularB ServiceCircularB);
public record class ServiceCircularB(ServiceCircularA ServiceCircularA);

public record class ServiceCircularConstructor
{
    public ServiceCircularConstructor(Injector injector)
    {
        injector.Get<ServiceCircularConstructor>();
    }
}

public record class ServiceUseInjectorInConstructor
{
    public readonly ServiceD ServiceD;

    public ServiceUseInjectorInConstructor(Injector injector)
    {
        ServiceD = injector.Get<ServiceD>();
    }
}

public record class ServiceThrowInConstructor
{
    public ServiceThrowInConstructor(Injector injector)
    {
        throw new NotImplementedException();
    }
}

public record struct StructService(ServiceA ServiceA, ServiceB ServiceB)
{
    public readonly Random Rng = new();
}

public record class StructServiceUser(StructService StructService);

public record class ServiceBad
{
    public ServiceBad(ServiceA serviceA)
    {
        _ = serviceA;
    }

    public ServiceBad(ServiceA serviceA, ServiceB serviceB)
    {
        _ = serviceA;
        _ = serviceB;
    }
}

public record class ServiceNoConstructor
{
    private ServiceNoConstructor() { }
}

public record class ServiceInvalidDependencies(int Dep);

[Valid]
[SubValid]
public record class ServiceTooManyAttributes;

public class CustomHandler : InjectorCustomHandler
{
    public override bool Handles(Type type) => type == typeof(CustomService);

    public override object Instantiate(Type type, InjectorScopeState state, InjectorPath path) =>
        new CustomService("Injected");
}

public class CustomHandlerThrows : InjectorCustomHandler
{
    public override bool Handles(Type type) => type == typeof(CustomService);

    public override object Instantiate(Type type, InjectorScopeState state, InjectorPath path) =>
        throw new NotImplementedException();
}

public class CustomHandlerHandlesThrows : InjectorCustomHandler
{
    public override bool Handles(Type type) => throw new NotImplementedException();

    public override object Instantiate(Type type, InjectorScopeState state, InjectorPath path) =>
        throw new NotImplementedException();
}

public class CustomHandlerWrongType : InjectorCustomHandler
{
    public override bool Handles(Type type) => type == typeof(CustomService);

    public override object Instantiate(Type type, InjectorScopeState state, InjectorPath path) => new();
}

public record class CustomService(string Message);

public class SecondCustomHandler : InjectorCustomHandler
{
    public override bool Handles(Type type) => type == typeof(SecondCustomService);

    public override object Instantiate(Type type, InjectorScopeState state, InjectorPath path) =>
        new SecondCustomService("Injected Also");
}

public record class SecondCustomService(string Message);

public class ScopedCustomHandler : InjectorCustomHandler
{
    public override bool Handles(Type type) => type == typeof(ScopedCustomService);

    public override object Instantiate(Type type, InjectorScopeState state, InjectorPath path) =>
        new ScopedCustomService("Injected Scoped");
}

[Valid]
public record class ScopedCustomService(string Message);

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class ValidAttribute : InjectorAttribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
public class SubValidAttribute : InjectorAttribute;

[Valid]
public class ValidScope : InjectorScope<ValidAttribute> { }

[Valid]
public record class ScopedService(ServiceE ServiceE);

[Valid]
public record class SeededScopedService(ScopedService ScopedService);

[Valid]
public record class InvalidScopedService(SubScopedService SubScopedService);

[SubValid]
public class SubValidScope : InjectorScope<SubValidAttribute> { }

[SubValid]
public record class SubScopedService(ScopedService ScopedService, ServiceE ServiceE);

public class NonScopedScope : InjectorScope<ValidAttribute>;

public class NoParameterlessConstructorScope : InjectorScope<ValidAttribute>
{
    public NoParameterlessConstructorScope(int value)
    {
        _ = value;
    }
}

public class PrivateConstructorScope : InjectorScope<ValidAttribute>
{
    private PrivateConstructorScope() { }
}

public class NonGenericInjectorScope : InjectorScope { }

public class Base<T> { }

public class Derived<T> : Base<T> { }
