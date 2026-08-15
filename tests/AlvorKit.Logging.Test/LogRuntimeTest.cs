namespace AlvorKit;

/// <summary>Exercises application-log formatting, filtering, buffering, and lifecycle behavior.</summary>
[TestClass]
public sealed class LogRuntimeTest
{
    /// <summary>Level filtering preserves raw output and formats accepted entries with caller information.</summary>
    [TestMethod]
    public void Flush_FiltersAndFormatsEntries()
    {
        using var output = new StringWriter();
        using var runtime = new LogRuntime(output) { UseColor = false };
        runtime.Log.Level = LogLevel.Info;
        Assert.AreEqual(LogLevel.Info, runtime.Log.Level);
        Assert.IsFalse(runtime.UseColor);

        runtime.Log.Debug("hidden");
        runtime.Log.Info("Value {0}", 42);
        runtime.Log.Raw("raw");
        runtime.Flush();

        var text = output.ToString();
        Assert.IsFalse(text.Contains("hidden", StringComparison.Ordinal));
        StringAssert.Contains(text, "[INFO] [LogRuntimeTest.cs:");
        StringAssert.Contains(text, "Value 42");
        StringAssert.EndsWith(text, $"raw{Environment.NewLine}");
    }

    /// <summary>Exception details and messages larger than a normal segment survive a synchronous flush.</summary>
    [TestMethod]
    public void Flush_WritesExceptionAndOversizedMessage()
    {
        using var output = new StringWriter();
        using var runtime = new LogRuntime(output) { UseColor = false };
        var oversized = new string('x', 70_000);

        runtime.Log.Error("Failure", new InvalidOperationException("broken"));
        runtime.Log.Raw(oversized);
        runtime.Flush();

        var text = output.ToString();
        StringAssert.Contains(text, "Failure");
        StringAssert.Contains(text, "System.InvalidOperationException: broken");
        StringAssert.Contains(text, oversized);
    }

    /// <summary>Stopping a running collector drains entries published by every producer thread.</summary>
    [TestMethod]
    public void Stop_DrainsConcurrentProducers()
    {
        const int producerCount = 4;
        const int entriesPerProducer = 5_000;
        using var output = new CountingWriter();
        using var runtime = new LogRuntime(output) { UseColor = false };
        runtime.Start();
        runtime.Start();

        var threads = Enumerable.Range(0, producerCount)
            .Select(worker => new Thread(() =>
            {
                for (int entry = 0; entry < entriesPerProducer; entry++)
                    runtime.Log.Info("worker={0} entry={1}", worker, entry);
            }))
            .ToArray();

        foreach (var thread in threads)
            thread.Start();
        foreach (var thread in threads)
            thread.Join();

        runtime.Stop();
        runtime.Stop();

        Assert.AreEqual(producerCount * entriesPerProducer, output.Lines);
    }

    /// <summary>Enabled console colors wrap severity text while raw output remains unstyled.</summary>
    [TestMethod]
    public void Flush_UsesConfiguredAnsiColors()
    {
        using var output = new StringWriter();
        using var runtime = new LogRuntime(output) { UseColor = true };

        runtime.Log.Warn("warning");
        runtime.Log.Raw("raw");
        runtime.Flush();

        var text = output.ToString();
        StringAssert.Contains(text, "\x1b[1;93;49m");
        StringAssert.Contains(text, "\x1b[0m");
        StringAssert.EndsWith(text, $"raw{Environment.NewLine}");
    }

    /// <summary>Filtered producer calls do not allocate after the generic call path is warm.</summary>
    [TestMethod]
    public void DisabledLevel_DoesNotAllocate()
    {
        using var runtime = new LogRuntime(TextWriter.Null);
        runtime.Log.Level = LogLevel.Off;
        runtime.Log.Debug("warm");

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1_000; i++)
            runtime.Log.Debug("hidden");
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.AreEqual(0, allocated);
    }

    /// <summary>Math values format correctly without RootText initialization or process-wide formatter registration.</summary>
    [TestMethod]
    public void Flush_WithMathValue_UsesItsSpanFormattingContract()
    {
        using var output = new StringWriter();
        using var runtime = new LogRuntime(output) { UseColor = false };
        Vec3 position = (1, 2, 3);

        runtime.Log.Info("position={0}", position);
        runtime.Flush();

        StringAssert.Contains(output.ToString(), "position=(1, 2, 3)");
    }

    /// <summary>Enabled math formatting allocates no producer-thread memory after type and buffer warmup.</summary>
    [TestMethod]
    public void EnabledMathFormatting_DoesNotAllocateAfterWarmup()
    {
        using var runtime = new LogRuntime(TextWriter.Null) { UseColor = false };
        Vec3 position = (1.25f, 2.5f, 3.75f);

        for (int i = 0; i < 10_000; i++)
        {
            runtime.Log.Info("position={0:F2}", position);
            if (i % 1_000 == 999)
                runtime.Flush();
        }

        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 20_000; i++)
        {
            runtime.Log.Info("position={0:F2}", position);
            if (i % 1_000 == 999)
                runtime.Flush();
        }
        long allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.AreEqual(0, allocated);
    }

    /// <summary>A flush racing with shutdown always completes after the worker drains its final entries.</summary>
    [TestMethod]
    public void Flush_RacingStop_Completes()
    {
        for (int iteration = 0; iteration < 100; iteration++)
        {
            using var runtime = new LogRuntime(TextWriter.Null);
            runtime.Start();
            runtime.Log.Info("iteration={0}", iteration);
            using var gate = new ManualResetEventSlim();
            var flush = Task.Run(() =>
            {
                gate.Wait();
                runtime.Flush();
            });
            var stop = Task.Run(() =>
            {
                gate.Wait();
                runtime.Stop();
            });

            gate.Set();
            Assert.IsTrue(Task.WaitAll([flush, stop], TimeSpan.FromSeconds(5)));
        }
    }

    private sealed class CountingWriter : TextWriter
    {
        private int lines;

        public override Encoding Encoding => Encoding.UTF8;
        public int Lines => lines;

        public override void Write(ReadOnlySpan<char> buffer)
        {
            foreach (char character in buffer)
            {
                if (character == '\n')
                    lines++;
            }
        }
    }
}
