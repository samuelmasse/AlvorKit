namespace AlvorKit.Text.Test;

/// <summary>Verifies composite parsing, typed formatting, reusable growth, and allocation behavior.</summary>
[TestClass]
public sealed class CompositeTextTest
{
    /// <summary>Composite text supports escaped braces, format specifiers, and left and right alignment.</summary>
    [TestMethod]
    public void Append_WithCompositeSyntax_FormatsExpectedText()
    {
        var buffer = new TextBuffer();

        CompositeText.Append(buffer, "{{{0,6:X4}}} {1,-5}!", 42, "ok");

        Assert.AreEqual("{  002A} ok   !", buffer.Span.ToString());
    }

    /// <summary>The one-argument fallback parser supports repetition, escaped braces, signs, whitespace, and narrow fields.</summary>
    [TestMethod]
    public void Append_WithDetailedSingleArgumentSyntax_UsesCompleteParser()
    {
        var buffer = new TextBuffer();

        CompositeText.Append(buffer, "{{{0, +6:X4}}} {0} {0,1}", 42);

        Assert.AreEqual("{  002A} 42 42", buffer.Span.ToString());
    }

    /// <summary>Direct buffer appends preserve substring, typed-value, null-string, and repeated-item behavior.</summary>
    [TestMethod]
    public void Buffer_WithDirectAppends_PreservesExpectedText()
    {
        var buffer = new TextBuffer();
        int value = 9;

        buffer.Append("0123", 1, 2);
        buffer.Append(in value);
        buffer.Append((string?)null);
        CompositeText.Append(buffer, "{0}{0}", 4);

        Assert.AreEqual("12944", buffer.Span.ToString());
    }

    /// <summary>Every strongly typed overload appends its indexed arguments in order.</summary>
    [TestMethod]
    public void Append_WithOneThroughEightArguments_PreservesOrder()
    {
        var buffer = new TextBuffer();

        CompositeText.Append(buffer, "{0}", 1);
        buffer.Append('|');
        CompositeText.Append(buffer, "{0}{1}", 1, 2);
        buffer.Append('|');
        CompositeText.Append(buffer, "{0}{1}{2}", 1, 2, 3);
        buffer.Append('|');
        CompositeText.Append(buffer, "{0}{1}{2}{3}", 1, 2, 3, 4);
        buffer.Append('|');
        CompositeText.Append(buffer, "{0}{1}{2}{3}{4}", 1, 2, 3, 4, 5);
        buffer.Append('|');
        CompositeText.Append(buffer, "{0}{1}{2}{3}{4}{5}", 1, 2, 3, 4, 5, 6);
        buffer.Append('|');
        CompositeText.Append(buffer, "{0}{1}{2}{3}{4}{5}{6}", 1, 2, 3, 4, 5, 6, 7);
        buffer.Append('|');
        CompositeText.Append(buffer, "{0}{1}{2}{3}{4}{5}{6}{7}", 1, 2, 3, 4, 5, 6, 7, 8);

        Assert.AreEqual("1|12|123|1234|12345|123456|1234567|12345678", buffer.Span.ToString());
    }

    /// <summary>String builders and read-only memory copy directly without formatter registration.</summary>
    [TestMethod]
    public void Append_WithTextContainers_CopiesTheirCharacters()
    {
        var buffer = new TextBuffer();
        var builder = new StringBuilder("builder");
        ReadOnlyMemory<char> memory = "memory".AsMemory();

        CompositeText.Append(buffer, "{0} {1}", builder, memory);

        Assert.AreEqual("builder memory", buffer.Span.ToString());
    }

    /// <summary>Values without span formatting retain null, formatted fallback, and plain ToString semantics.</summary>
    [TestMethod]
    public void Append_WithFallbackValues_UsesTheirAvailableContracts()
    {
        var buffer = new TextBuffer();
        int? missingNumber = null;
        TestReferenceSpanFormattable? missingReference = null;

        CompositeText.Append(buffer, "plain", 1);
        CompositeText.Append(buffer, "|{0}|", missingNumber);
        CompositeText.Append(buffer, "{0:X4}", new TestFallbackFormattable(42));
        CompositeText.Append(buffer, "|{0}|", new TestPlainValue(7));
        CompositeText.Append(buffer, "{0}", missingReference);

        Assert.AreEqual("plain||002A|value=7|", buffer.Span.ToString());
    }

    /// <summary>Retained storage grows for long values and can be cleared for immediate reuse.</summary>
    [TestMethod]
    public void Append_WhenCapacityIsExceeded_RetainsCompleteText()
    {
        var buffer = new TextBuffer(1);
        var value = new string('x', 1_024);

        CompositeText.Append(buffer, "<{0}>", value);
        Assert.AreEqual($"<{value}>", buffer.Span.ToString());

        buffer.Clear();
        buffer.Append("again");
        Assert.AreEqual("again", buffer.Span.ToString());
    }

    /// <summary>A span formatter can request retained buffer growth and then complete without a fallback string.</summary>
    [TestMethod]
    public void Append_WhenSpanFormatterNeedsMoreSpace_RetriesWithGrowth()
    {
        var buffer = new TextBuffer(0);

        CompositeText.Append(buffer, "<{0}>", new LongSpanFormattable());

        Assert.AreEqual($"<{new string('x', LongSpanFormattable.Length)}>", buffer.Span.ToString());
    }

    /// <summary>Malformed braces, alignments, and unavailable argument indexes are rejected.</summary>
    [TestMethod]
    public void Append_WithInvalidFormat_ThrowsFormatException()
    {
        var buffer = new TextBuffer();

        Assert.Throws<FormatException>(() => CompositeText.Append(buffer, "{", 1));
        Assert.Throws<FormatException>(() => CompositeText.Append(buffer, "}", 1));
        Assert.Throws<FormatException>(() => CompositeText.Append(buffer, "{0,}", 1));
        Assert.Throws<FormatException>(() => CompositeText.Append(buffer, "{1}", 1));
        Assert.Throws<FormatException>(() => CompositeText.Append(buffer, "{0:bad", 1));
        Assert.Throws<FormatException>(() => CompositeText.Append(buffer, "{0x}", 1));
        Assert.Throws<FormatException>(() => CompositeText.Append(buffer, "{999999999999}", 1));
        Assert.Throws<FormatException>(() => CompositeText.Append(buffer, "bad}", 1, 2));
        Assert.Throws<FormatException>(() => CompositeText.Append(buffer, "{2}", 1, 2));
        Assert.Throws<ArgumentOutOfRangeException>(() => _ = new TextBuffer(-1));
    }

    /// <summary>Span-formattable values allocate no managed memory after type and buffer warmup.</summary>
    [TestMethod]
    public void Append_WithSpanFormattable_DoesNotAllocateAfterWarmup()
    {
        var buffer = new TextBuffer();
        var value = new TestSpanFormattable(42);

        for (int i = 0; i < 1_000; i++)
        {
            buffer.Clear();
            CompositeText.Append(buffer, "value={0:X4}", value);
        }

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 10_000; i++)
        {
            buffer.Clear();
            CompositeText.Append(buffer, "value={0:X4}", value);
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.AreEqual(0, allocated);
        Assert.AreEqual("value=002A", buffer.Span.ToString());
    }
}

/// <summary>Provides a test value with only the standard span-formatting contract.</summary>
internal readonly record struct TestSpanFormattable(int Value) : ISpanFormattable
{
    /// <summary>Formats the value into a newly allocated string for non-hot-path callers.</summary>
    public string ToString(string? format, IFormatProvider? formatProvider) =>
        Value.ToString(format, formatProvider);

    /// <summary>Formats the value directly into caller-owned character storage.</summary>
    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider) =>
        Value.TryFormat(destination, out charsWritten, format, provider);
}

/// <summary>Provides a deliberately long span-formatted value that exercises retained growth.</summary>
internal readonly struct LongSpanFormattable : ISpanFormattable
{
    /// <summary>Defines the formatted character count.</summary>
    public const int Length = 100;

    /// <summary>Creates the allocating representation for non-hot-path callers.</summary>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        _ = format;
        _ = formatProvider;
        return new string('x', Length);
    }

    /// <summary>Requests more space until the destination can hold the complete value.</summary>
    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        _ = format;
        _ = provider;
        if (destination.Length < Length)
        {
            charsWritten = 0;
            return false;
        }

        destination[..Length].Fill('x');
        charsWritten = Length;
        return true;
    }
}

/// <summary>Provides only the allocating <see cref="IFormattable"/> contract.</summary>
internal sealed class TestFallbackFormattable(int value) : IFormattable
{
    /// <summary>Formats the stored integer through the requested fallback format.</summary>
    public string ToString(string? format, IFormatProvider? formatProvider) =>
        value.ToString(format, formatProvider);
}

/// <summary>Provides only a plain object string representation.</summary>
internal sealed class TestPlainValue(int value)
{
    /// <summary>Returns the plain fallback representation.</summary>
    public override string ToString() => $"value={value}";
}

/// <summary>Provides a nullable reference type with the span-formatting contract.</summary>
internal sealed class TestReferenceSpanFormattable : ISpanFormattable
{
    /// <summary>Returns an empty allocating representation.</summary>
    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        _ = format;
        _ = formatProvider;
        return string.Empty;
    }

    /// <summary>Formats an empty value into caller-owned storage.</summary>
    public bool TryFormat(
        Span<char> destination,
        out int charsWritten,
        ReadOnlySpan<char> format,
        IFormatProvider? provider)
    {
        _ = destination;
        _ = format;
        _ = provider;
        charsWritten = 0;
        return true;
    }
}
