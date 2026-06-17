namespace AlvorKit.Mocking;

/// <summary>Provides argument matchers for mocked value, reference, and by-ref parameters.</summary>
public static class Arg
{
    /// <summary>Matches any argument value of the requested type while configuring a mocked call.</summary>
    public static T Any<T>() where T : allows ref struct
    {
        if (Capture.Context.IsActive)
            Capture.WriteMatcher(new(MatcherType.Any, null));

        return Value<T>();
    }

    /// <summary>Matches arguments accepted by a predicate while configuring a mocked call.</summary>
    public static T Match<T>(Func<T, bool> func)
    {
        if (Capture.Context.IsActive)
        {
            Func<object, bool> f = o => func.Invoke((T)o);
            Capture.WriteMatcher(new(MatcherType.Func, f));
        }

        return Value<T>();
    }

    /// <summary>Returns the sentinel value used for matcher capture or the default value outside capture.</summary>
    internal static T Value<T>() where T : allows ref struct
    {
        if (Capture.Context.IsDisambiguating)
            return Ones<T>();
        else return default!;
    }

    /// <summary>Returns a bitwise all-ones sentinel that can be fingerprinted during matcher disambiguation.</summary>
    internal static T Ones<T>() where T : allows ref struct
    {
        Span<byte> buffer = stackalloc byte[Unsafe.SizeOf<T>()];
        buffer.Fill(0xFF);
        return Unsafe.As<byte, T>(ref buffer[0]);
    }
}

/// <summary>Provides by-reference argument matchers for mocked ref and out parameters.</summary>
public static class Arg<T>
{
    /// <summary>Default value storage returned when a by-reference matcher is evaluated outside capture disambiguation.</summary>
    private static T zero = default!;

    /// <summary>Unmanaged all-ones storage returned while by-reference matcher positions are disambiguated.</summary>
    private static unsafe readonly byte* one = (byte*)NativeMemory.Alloc((nuint)Unsafe.SizeOf<T>());

    /// <summary>Matches any by-reference argument value while configuring a mocked call.</summary>
    public static ref T Any()
    {
        Arg.Any<T>();
        return ref Value();
    }

    /// <summary>Matches by-reference arguments accepted by a predicate while configuring a mocked call.</summary>
    public static ref T Match(Func<T, bool> func)
    {
        Arg.Match(func);
        return ref Value();
    }

    /// <summary>Returns by-reference storage appropriate for the current capture phase.</summary>
    internal unsafe static ref T Value()
    {
        if (Capture.Context.IsDisambiguating)
        {
            new Span<byte>(one, Unsafe.SizeOf<T>()).Fill(0xFF);
            return ref Unsafe.AsRef<T>(one);
        }
        else
        {
            zero = default!;
            return ref zero;
        }
    }
}
