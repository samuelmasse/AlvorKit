namespace AlvorKit;

/// <summary>Provides distinct closed interface types for cache-cold proxy generation samples.</summary>
public interface IColdProxyTarget<TTag>
{
    /// <summary>Returns a value derived from one ordinary argument.</summary>
    int Invoke(int value);
}
