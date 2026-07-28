namespace AlvorKit.Mocking.Demo;

/// <summary>Rendering collaborator used for strict, loose, matcher, and callback examples.</summary>
public interface IRenderer
{
    /// <summary>Draws a sprite on a logical layer.</summary>
    bool Draw(string sprite, int layer);
}

/// <summary>Input stage used in the cross-mock frame sequence.</summary>
public interface IFrameInput
{
    /// <summary>Polls input for the current frame.</summary>
    void Poll();
}

/// <summary>Audio stage used in the cross-mock frame sequence.</summary>
public interface IAudioMixer
{
    /// <summary>Mixes audio for the current frame.</summary>
    void Mix();
}

/// <summary>Span-consuming collaborator used for typed callbacks and stable snapshots.</summary>
public interface ISampleAnalyzer
{
    /// <summary>Returns the sum of the supplied borrowed samples.</summary>
    int Sum(ReadOnlySpan<int> values);
}

/// <summary>Mutable object used to demonstrate configured and original partial calls.</summary>
public class Counter
{
    /// <summary>Current value retained by original counter behavior.</summary>
    public int Current;

    /// <summary>Advances the counter by one.</summary>
    public int Next() => ++Current;

    /// <summary>Advances the counter by the supplied amount.</summary>
    public int Add(int amount) => Current += amount;
}

/// <summary>Resource collaborator used for failures and return sequences.</summary>
public interface IResourceCatalog
{
    /// <summary>Loads one named resource.</summary>
    object Load(string name);

    /// <summary>Returns the next retry delay.</summary>
    int NextRetryDelay();
}

/// <summary>Frame signals used for event raising and ordinary reference writeback.</summary>
public interface IFrameSignals
{
    /// <summary>Raised when a frame is ready.</summary>
    event Action<int> FrameReady;

    /// <summary>Reads one value while advancing the caller's offset.</summary>
    bool TryRead(
        string name,
        ref int offset,
        out string value);
}

/// <summary>Borrowed buffer operations used for typed span behavior.</summary>
public interface IBufferOperations
{
    /// <summary>Fills caller-owned storage and returns the written count.</summary>
    int Fill(Span<int> destination);

    /// <summary>Returns a borrowed read-only view.</summary>
    ReadOnlySpan<int> Borrow();
}

/// <summary>Stable storage owner for a borrowed return factory.</summary>
public sealed class BufferOwner(int[] values)
{
    /// <summary>Returns a view over the owner's stable array.</summary>
    public ReadOnlySpan<int> Borrow() => values;
}

/// <summary>Async collaborator whose borrowed input must be copied before suspension.</summary>
public interface IAsyncSampleAnalyzer
{
    /// <summary>Returns an asynchronous sum of borrowed input.</summary>
    Task<int> SumAsync(ReadOnlySpan<int> values);
}

/// <summary>Class proxy used to demonstrate automatic generic construction setup.</summary>
public class GenericFormatter
{
    /// <summary>Formats one constructed generic input.</summary>
    public virtual string Format<T>(T value) =>
        value?.ToString() ?? string.Empty;
}
