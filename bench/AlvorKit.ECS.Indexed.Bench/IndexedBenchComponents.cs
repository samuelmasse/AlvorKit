namespace AlvorKit;

public class BenchPlain : IComponent
{
    public static EntComponent Component => new(typeof(int), typeof(BenchPlain), false);
}

public class BenchScalar : IComponent
{
    public static EntComponent Component => new(typeof(int), typeof(BenchScalar), false);
}

public class BenchArray : IComponent
{
    public static EntComponent Component => new(typeof(int[]), typeof(BenchArray), false);
}

public class BenchDirty : IComponent
{
    public static EntComponent Component => new(typeof(bool), typeof(BenchDirty), false);
}

public class BenchActive : IComponent
{
    public static EntComponent Component => new(typeof(bool), typeof(BenchActive), false);
}

public class BenchGate : IComponent
{
    public static EntComponent Component => new(typeof(bool), typeof(BenchGate), false);
}

public class BenchKey : IComponent
{
    public static EntComponent Component => new(typeof(int), typeof(BenchKey), false);
}

public readonly record struct BenchWideValue(long A, long B, long C, long D, long E, long F, long G, long H);

public class BenchWide : IComponent
{
    public static EntComponent Component => new(typeof(BenchWideValue), typeof(BenchWide), false);
}
