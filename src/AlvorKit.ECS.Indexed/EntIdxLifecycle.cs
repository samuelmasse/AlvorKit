namespace AlvorKit;

internal abstract class EntIdxLifecycle : IComponent
{
    public static EntComponent Component => new(typeof(EntIdxLifecycleCallbacks), typeof(EntIdxLifecycle), false);
}
