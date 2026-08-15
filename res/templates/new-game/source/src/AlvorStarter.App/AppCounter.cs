namespace AlvorStarter;

/// <summary>Stores the starter UI click count.</summary>
[App]
public class AppCounter(Log log)
{
    private int value;

    /// <summary>Gets the number of button clicks observed by the menu.</summary>
    public int Value => value;

    /// <summary>Increments the click count.</summary>
    public void Increment()
    {
        value++;
        log.Debug("Starter click count is {0}", value);
    }

    /// <summary>Resets the click count.</summary>
    public void Reset() => value = 0;
}
