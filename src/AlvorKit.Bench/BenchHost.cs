namespace AlvorKit;

/// <summary>Composes one suite and executes its command line in an isolated benchmark scope.</summary>
public static class BenchHost
{
    /// <summary>Parses arguments and runs the requested suite through benchmark-scope injection.</summary>
    public static int Run<TSuite>(string[] args) where TSuite : class, IBenchSuiteProvider =>
        new BenchCommandLine(Execute<TSuite>).Invoke(args);

    /// <summary>Creates one scope with invocation options and binds the suite provider.</summary>
    private static int Execute<TSuite>(BenchCommand command) where TSuite : class, IBenchSuiteProvider
    {
        var bench = new Injector()
            .Scope<BenchScope>()
            .With(command)
            .With(BenchAllocationTracking.Connect());
        bench.Bind<TSuite>();
        return bench.Get<BenchApplication>().Run();
    }
}
