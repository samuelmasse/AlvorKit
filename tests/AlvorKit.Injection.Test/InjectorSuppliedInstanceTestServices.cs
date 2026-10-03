namespace AlvorKit;

public class SuppliedGenerator : ISharedGenerator
{
    public string Name => "supplied";
}

[Binding]
public class SuppliedGeneratorConsumer(SuppliedGenerator generator)
{
    public SuppliedGenerator Generator => generator;
}

[OtherBinding]
public class SuppliedGeneratorChildConsumer(SuppliedGenerator generator)
{
    public SuppliedGenerator Generator => generator;
}

[OtherBinding]
public class SuppliedGeneratorAliasConsumer(ISharedGenerator generator)
{
    public ISharedGenerator Generator => generator;
}

public class SuppliedContainer(ServiceA service)
{
    public ServiceA Service => service;
}

[Binding]
public class ScopedServiceAConsumer(ServiceA service)
{
    public ServiceA Service => service;
}

public class UnmarkedBiomeGenerator : IBindingBiomeGenerator
{
    public string Name => "unmarked";
}

public class SuppliedInstanceObserver : IInjectorInstanceObserver
{
    private readonly List<(InjectorScope Owner, object Instance)> instances = [];

    public IReadOnlyList<(InjectorScope Owner, object Instance)> Instances => instances;

    public void OnInstanceOwned(InjectorScope owner, object instance) => instances.Add((owner, instance));
}
