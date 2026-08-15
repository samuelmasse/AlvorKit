namespace AlvorKit;

/// <summary>Observes dependency instances without taking ownership of them.</summary>
public interface IInjectorInstanceObserver
{
    /// <summary>Reports the exact scope that constructed or accepted an instance.</summary>
    void OnInstanceOwned(InjectorScope owner, object instance);
}
