namespace AlvorKit.Logging.Test;

/// <summary>Verifies the complete severity and typed-format overload matrix.</summary>
[TestClass]
public sealed class LogOverloadTest
{
    /// <summary>Every preserved overload obeys filtering and writes its expected formatted value when enabled.</summary>
    [TestMethod]
    public void SeverityOverloads_FilterAndFormatEveryArity()
    {
        using var output = new StringWriter();
        using var runtime = new LogRuntime(output) { UseColor = false };
        runtime.Log.Level = LogLevel.Off;

        WriteEveryOverload(runtime.Log);
        runtime.Flush();
        Assert.AreEqual("", output.ToString());

        runtime.Log.Level = LogLevel.All;
        WriteEveryOverload(runtime.Log);
        runtime.Flush();

        var text = output.ToString();
        AssertLevel(text, "FATAL", "fatal");
        AssertLevel(text, "ERROR", "error");
        AssertLevel(text, "WARN", "warn");
        AssertLevel(text, "INFO", "info");
        AssertLevel(text, "DEBUG", "debug");
        AssertLevel(text, "TRACE", "trace");
    }

    private static void WriteEveryOverload(Log log)
    {
        WriteFatal(log);
        WriteError(log);
        WriteWarn(log);
        WriteInfo(log);
        WriteDebug(log);
        WriteTrace(log);
    }

    private static void WriteFatal(Log log)
    {
        log.Fatal(new InvalidOperationException("fatal-exception"));
        log.Fatal("fatal-plain");
        log.Fatal(new TaggedValue("fatal"));
        log.Fatal("fatal-arity1:{0}", 1);
        log.Fatal("fatal-arity2:{0}{1}", 1, 2);
        log.Fatal("fatal-arity3:{0}{1}{2}", 1, 2, 3);
        log.Fatal("fatal-arity4:{0}{1}{2}{3}", 1, 2, 3, 4);
        log.Fatal("fatal-arity5:{0}{1}{2}{3}{4}", 1, 2, 3, 4, 5);
        log.Fatal("fatal-arity6:{0}{1}{2}{3}{4}{5}", 1, 2, 3, 4, 5, 6);
        log.Fatal("fatal-arity7:{0}{1}{2}{3}{4}{5}{6}", 1, 2, 3, 4, 5, 6, 7);
        log.Fatal("fatal-arity8:{0}{1}{2}{3}{4}{5}{6}{7}", 1, 2, 3, 4, 5, 6, 7, 8);
    }

    private static void WriteError(Log log)
    {
        log.Error(new InvalidOperationException("error-exception"));
        log.Error("error-plain");
        log.Error(new TaggedValue("error"));
        log.Error("error-arity1:{0}", 1);
        log.Error("error-arity2:{0}{1}", 1, 2);
        log.Error("error-arity3:{0}{1}{2}", 1, 2, 3);
        log.Error("error-arity4:{0}{1}{2}{3}", 1, 2, 3, 4);
        log.Error("error-arity5:{0}{1}{2}{3}{4}", 1, 2, 3, 4, 5);
        log.Error("error-arity6:{0}{1}{2}{3}{4}{5}", 1, 2, 3, 4, 5, 6);
        log.Error("error-arity7:{0}{1}{2}{3}{4}{5}{6}", 1, 2, 3, 4, 5, 6, 7);
        log.Error("error-arity8:{0}{1}{2}{3}{4}{5}{6}{7}", 1, 2, 3, 4, 5, 6, 7, 8);
    }

    private static void WriteWarn(Log log)
    {
        log.Warn(new InvalidOperationException("warn-exception"));
        log.Warn("warn-plain");
        log.Warn(new TaggedValue("warn"));
        log.Warn("warn-arity1:{0}", 1);
        log.Warn("warn-arity2:{0}{1}", 1, 2);
        log.Warn("warn-arity3:{0}{1}{2}", 1, 2, 3);
        log.Warn("warn-arity4:{0}{1}{2}{3}", 1, 2, 3, 4);
        log.Warn("warn-arity5:{0}{1}{2}{3}{4}", 1, 2, 3, 4, 5);
        log.Warn("warn-arity6:{0}{1}{2}{3}{4}{5}", 1, 2, 3, 4, 5, 6);
        log.Warn("warn-arity7:{0}{1}{2}{3}{4}{5}{6}", 1, 2, 3, 4, 5, 6, 7);
        log.Warn("warn-arity8:{0}{1}{2}{3}{4}{5}{6}{7}", 1, 2, 3, 4, 5, 6, 7, 8);
    }

    private static void WriteInfo(Log log)
    {
        log.Info(new InvalidOperationException("info-exception"));
        log.Info("info-plain");
        log.Info(new TaggedValue("info"));
        log.Info("info-arity1:{0}", 1);
        log.Info("info-arity2:{0}{1}", 1, 2);
        log.Info("info-arity3:{0}{1}{2}", 1, 2, 3);
        log.Info("info-arity4:{0}{1}{2}{3}", 1, 2, 3, 4);
        log.Info("info-arity5:{0}{1}{2}{3}{4}", 1, 2, 3, 4, 5);
        log.Info("info-arity6:{0}{1}{2}{3}{4}{5}", 1, 2, 3, 4, 5, 6);
        log.Info("info-arity7:{0}{1}{2}{3}{4}{5}{6}", 1, 2, 3, 4, 5, 6, 7);
        log.Info("info-arity8:{0}{1}{2}{3}{4}{5}{6}{7}", 1, 2, 3, 4, 5, 6, 7, 8);
    }

    private static void WriteDebug(Log log)
    {
        log.Debug(new InvalidOperationException("debug-exception"));
        log.Debug("debug-plain");
        log.Debug(new TaggedValue("debug"));
        log.Debug("debug-arity1:{0}", 1);
        log.Debug("debug-arity2:{0}{1}", 1, 2);
        log.Debug("debug-arity3:{0}{1}{2}", 1, 2, 3);
        log.Debug("debug-arity4:{0}{1}{2}{3}", 1, 2, 3, 4);
        log.Debug("debug-arity5:{0}{1}{2}{3}{4}", 1, 2, 3, 4, 5);
        log.Debug("debug-arity6:{0}{1}{2}{3}{4}{5}", 1, 2, 3, 4, 5, 6);
        log.Debug("debug-arity7:{0}{1}{2}{3}{4}{5}{6}", 1, 2, 3, 4, 5, 6, 7);
        log.Debug("debug-arity8:{0}{1}{2}{3}{4}{5}{6}{7}", 1, 2, 3, 4, 5, 6, 7, 8);
    }

    private static void WriteTrace(Log log)
    {
        log.Trace(new InvalidOperationException("trace-exception"));
        log.Trace("trace-plain");
        log.Trace(new TaggedValue("trace"));
        log.Trace("trace-arity1:{0}", 1);
        log.Trace("trace-arity2:{0}{1}", 1, 2);
        log.Trace("trace-arity3:{0}{1}{2}", 1, 2, 3);
        log.Trace("trace-arity4:{0}{1}{2}{3}", 1, 2, 3, 4);
        log.Trace("trace-arity5:{0}{1}{2}{3}{4}", 1, 2, 3, 4, 5);
        log.Trace("trace-arity6:{0}{1}{2}{3}{4}{5}", 1, 2, 3, 4, 5, 6);
        log.Trace("trace-arity7:{0}{1}{2}{3}{4}{5}{6}", 1, 2, 3, 4, 5, 6, 7);
        log.Trace("trace-arity8:{0}{1}{2}{3}{4}{5}{6}{7}", 1, 2, 3, 4, 5, 6, 7, 8);
    }

    private static void AssertLevel(string text, string level, string prefix)
    {
        Assert.AreEqual(11, Count(text, $"[{level}]"));
        StringAssert.Contains(text, $"{prefix}-exception");
        StringAssert.Contains(text, $"{prefix}-plain");
        StringAssert.Contains(text, $"{prefix}-value");

        for (int arity = 1; arity <= 8; arity++)
            StringAssert.Contains(text, $"{prefix}-arity{arity}:{string.Concat(Enumerable.Range(1, arity))}");
    }

    private static int Count(string text, string value)
    {
        int count = 0;
        int start = 0;
        while ((start = text.IndexOf(value, start, StringComparison.Ordinal)) >= 0)
        {
            count++;
            start += value.Length;
        }

        return count;
    }

    private readonly record struct TaggedValue(string Prefix)
    {
        public override string ToString() => $"{Prefix}-value";
    }
}
