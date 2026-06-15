namespace AlvorKit.Script.Bindgen.Test;

/// <summary>Tests for skipped-function console summaries.</summary>
[TestClass]
public sealed class SkippedFunctionReporterTest
{
    /// <summary>Reporter writes nothing when no native functions were skipped.</summary>
    [TestMethod]
    public void Print_NoSkippedFunctions_WritesNothing()
    {
        using var capture = new ConsoleCapture();

        SkippedFunctionReporter.Print([]);

        Assert.AreEqual("", capture.Output);
    }

    /// <summary>Reporter writes each skipped native function on its own indented line.</summary>
    [TestMethod]
    public void Print_SkippedFunctions_WritesSummary()
    {
        using var capture = new ConsoleCapture();

        SkippedFunctionReporter.Print(["glSkippedOne", "glSkippedTwo"]);

        Assert.AreEqual(
            $"Skipped functions:{Environment.NewLine}  glSkippedOne{Environment.NewLine}  glSkippedTwo{Environment.NewLine}",
            capture.Output);
    }

    /// <summary>Captures console output for one test and restores the original writer on disposal.</summary>
    private sealed class ConsoleCapture : IDisposable
    {
        /// <summary>Original console writer restored after the test.</summary>
        private readonly TextWriter original = Console.Out;

        /// <summary>Writer receiving console output during the test.</summary>
        private readonly StringWriter writer = new();

        /// <summary>Starts capturing console output.</summary>
        public ConsoleCapture()
        {
            Console.SetOut(writer);
        }

        /// <summary>Captured console text.</summary>
        public string Output => writer.ToString();

        /// <summary>Restores the original console writer.</summary>
        public void Dispose()
        {
            Console.SetOut(original);
            writer.Dispose();
        }
    }
}
