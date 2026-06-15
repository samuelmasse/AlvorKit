namespace AlvorKit.Script.Bindgen;

/// <summary>Entry point for the binding generator.</summary>
[ExcludeFromCodeCoverage]
internal static class Program
{
    /// <summary>Runs binding generation for the requested native library selection.</summary>
    public static async Task Main(string[] args)
    {
        var options = BindgenOptions.Parse(args);
        var repository = RepositoryLayout.FindFrom(AppContext.BaseDirectory);
        await new BindingGenerationRunner(repository, options).RunAsync();
    }
}
