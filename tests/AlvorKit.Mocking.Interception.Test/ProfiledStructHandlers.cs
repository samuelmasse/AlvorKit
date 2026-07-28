namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Counts and enters one writable struct wrapper.</summary>
internal sealed class ProfiledStructInt32Handler<T>(
    ProfiledStructInt32Operation<T> wrapper) :
    ProfiledReceiverFreeHandler
    where T : struct
{
    public int Invoke(ref T target, int amount)
    {
        Record();
        return wrapper(ref target, amount);
    }
}

/// <summary>Counts and enters one readonly struct wrapper.</summary>
internal sealed class ProfiledStructReadOnlyInt32Handler<T>(
    ProfiledStructReadOnlyInt32Operation<T> wrapper) :
    ProfiledReceiverFreeHandler
    where T : struct
{
    public int Invoke(in T target, int amount)
    {
        Record();
        return wrapper(in target, amount);
    }
}

/// <summary>Counts and enters one borrowed-window struct wrapper.</summary>
internal sealed class ProfiledStructWindowHandler(
    ProfiledStructWindowOperation wrapper) :
    ProfiledReceiverFreeHandler
{
    public ProfiledStructWindow Invoke(
        ref ProfiledMutableStructTarget target,
        int[] owner)
    {
        Record();
        return wrapper(ref target, owner);
    }
}
