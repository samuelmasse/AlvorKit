namespace AlvorKit;

public class EntIdxWideComponent : IComponent
{
    public static EntComponent Component => new(typeof(EntIdxWideValue), typeof(EntIdxWideComponent), false);
}
