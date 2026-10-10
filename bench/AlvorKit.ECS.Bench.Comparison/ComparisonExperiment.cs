namespace AlvorKit;

/// <summary>A complete measured contract, independent of display paths and report code.</summary>
internal record ComparisonExperiment(
    string Id,
    string Scenario,
    string Framework,
    string Storage,
    string Variant,
    string Mode,
    ComparisonInput Input,
    string Unit,
    int Operations,
    string Description,
    string Timing,
    bool Representative,
    Type Workload,
    MethodInfo MeasurementMethod,
    Func<BenchResult> Measure)
{
    private IdentityState identity = new()
    {
        Id = Id,
        Scenario = Scenario,
        Framework = Framework,
        Storage = Storage,
        Variant = Variant,
        Mode = Mode,
    };
    private ContractState contract = new()
    {
        Input = Input,
        Unit = Unit,
        Operations = Operations,
        Description = Description,
        Timing = Timing,
        Representative = Representative,
    };
    private ExecutionState execution = new()
    {
        Workload = Workload,
        MeasurementMethod = MeasurementMethod,
        Measure = Measure,
    };

    public string Id { get => identity.Id; init => identity.Id = value; }
    public string Scenario { get => identity.Scenario; init => identity.Scenario = value; }
    public string Framework { get => identity.Framework; init => identity.Framework = value; }
    public string Storage { get => identity.Storage; init => identity.Storage = value; }
    public string Variant { get => identity.Variant; init => identity.Variant = value; }
    public string Mode { get => identity.Mode; init => identity.Mode = value; }
    public ComparisonInput Input { get => contract.Input; init => contract.Input = value; }
    public string Unit { get => contract.Unit; init => contract.Unit = value; }
    public int Operations { get => contract.Operations; init => contract.Operations = value; }
    public string Description { get => contract.Description; init => contract.Description = value; }
    public string Timing { get => contract.Timing; init => contract.Timing = value; }
    public bool Representative { get => contract.Representative; init => contract.Representative = value; }
    public Type Workload { get => execution.Workload; init => execution.Workload = value; }
    public MethodInfo MeasurementMethod { get => execution.MeasurementMethod; init => execution.MeasurementMethod = value; }
    public Func<BenchResult> Measure { get => execution.Measure; init => execution.Measure = value; }

    internal Func<ComparisonInput, BenchResult>? Parameterized { get; init; }

    internal ComparisonExperiment WithPasses(int passes)
    {
        if (Parameterized == null)
            throw new InvalidOperationException($"This operation has a fixed data scale: {Id}");

        if (passes <= 0 || passes > int.MaxValue / (Operations / Input.Passes))
            throw new ArgumentOutOfRangeException(nameof(passes));

        var input = Input with { Passes = passes };
        return this with { Input = input, Operations = Operations / Input.Passes * passes, Measure = () => Parameterized(input) };
    }

    internal object Metadata => new
    {
        Id, Scenario, Framework, Storage, Variant, Mode, Input, Unit, Operations, Description, Timing, Representative,
    };

    private struct IdentityState
    {
        public string Id;
        public string Scenario;
        public string Framework;
        public string Storage;
        public string Variant;
        public string Mode;
    }

    private struct ContractState
    {
        public ComparisonInput Input;
        public string Unit;
        public int Operations;
        public string Description;
        public string Timing;
        public bool Representative;
    }

    private struct ExecutionState
    {
        public Type Workload;
        public MethodInfo MeasurementMethod;
        public Func<BenchResult> Measure;
    }
}
