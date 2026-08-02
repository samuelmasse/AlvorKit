namespace AlvorKit.Injection.Test;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class BindingAttribute : InjectorAttribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface)]
public class OtherBindingAttribute : InjectorAttribute;

[Binding]
public class BindingScope : InjectorScope<BindingAttribute>;

[OtherBinding]
public class OtherBindingScope : InjectorScope<OtherBindingAttribute>;

[Binding]
public interface IBindingBiomeGenerator
{
    string Name { get; }
}

[Binding]
public abstract class BindingBiomeGeneratorBase
{
    public abstract string Name { get; }
}

[Binding]
public class BindingBiomeGenerator(ServiceA serviceA) : BindingBiomeGeneratorBase, IBindingBiomeGenerator
{
    public ServiceA ServiceA => serviceA;

    public override string Name => "binding";
}

[Binding]
public class AlternateBindingBiomeGenerator : BindingBiomeGeneratorBase, IBindingBiomeGenerator
{
    public override string Name => "alternate";
}

[Binding]
public class UnrelatedBindingService;

[Binding]
public class OpenBindingService<T>;

[Binding]
public class BindingTerrain(IBindingBiomeGenerator generator)
{
    public IBindingBiomeGenerator Generator => generator;
}

[Binding]
public class BindingBaseTerrain(BindingBiomeGeneratorBase generator)
{
    public BindingBiomeGeneratorBase Generator => generator;
}

public interface ISharedGenerator
{
    string Name { get; }
}

[Binding]
public class ParentSharedGenerator : ISharedGenerator
{
    public string Name => "parent";
}

[OtherBinding]
public class ChildSharedGenerator : ISharedGenerator
{
    public string Name => "child";
}

[OtherBinding]
public interface IOtherBindingGenerator;

[Binding]
public class BindingWrongSurfaceGenerator : IOtherBindingGenerator;

public interface IUnmarkedGenerator;

[OtherBinding]
public class OtherScopedGenerator : IUnmarkedGenerator;

[Binding]
public interface ICircularBindingService;

[Binding]
public class CircularBindingService(ICircularBindingService self) : ICircularBindingService
{
    public ICircularBindingService Self => self;
}

public interface IHandlerService
{
    string Message { get; }
}

public class HandlerService : IHandlerService
{
    public string Message => "handled";
}

public class InterfaceHandler : InjectorCustomHandler
{
    public override bool Handles(Type type) => type == typeof(IHandlerService);

    public override object Instantiate(Type type, InjectorScopeState state, InjectorPath path) =>
        new HandlerService();
}
