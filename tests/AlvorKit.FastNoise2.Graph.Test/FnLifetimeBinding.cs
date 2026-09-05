namespace AlvorKit;

/// <summary>Forwards to real FastNoise2 while observing external native-reference release.</summary>
internal class FnLifetimeBinding(Fn inner) : FnWrapper(inner)
{
    private int released;
    private Action? beforeSample;

    /// <summary>Gets the number of completed native releases, including finalizer-thread calls.</summary>
    internal int Released => Volatile.Read(ref released);

    /// <summary>Gets the optional collection probe run after the wrapper has borrowed a native reference.</summary>
    internal ref Action? BeforeSample => ref beforeSample;

    /// <summary>Releases the real external reference before recording its completion.</summary>
    public override void DeleteNodeRef(FnNode node)
    {
        base.DeleteNodeRef(node);
        Interlocked.Increment(ref released);
    }

    /// <summary>Runs the lifetime probe at the native boundary before sampling the real node.</summary>
    public override float GenSingle2D(FnNode node, float x, float y, int seed)
    {
        beforeSample?.Invoke();
        return base.GenSingle2D(node, x, y, seed);
    }
}
