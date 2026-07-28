/// <summary>Shows install, replacement, and removal changing a normal engine render loop live.</summary>
[Root]
internal sealed class LivePatchDemoState(
    RootGl gl,
    RootScreen screen,
    RootInput input,
    RootKeyboard keyboard,
    RootBackbuffer backbuffer,
    RootCanvas canvas,
    RootPositionColorProgram positionColorProgram) : State
{
    private InterceptionProfiler profiler = null!;
    private MethodInfo method = null!;
    private IInterceptionPatchHandle? patch;
    private InterceptionCompletion completion;
    private ulong observedRequest;
    private int nextMode = 1;
    private GlVertexArrayHandle vao;

    /// <inheritdoc />
    public override void Load()
    {
        PositionColorVertex[] vertices =
        [
            new((0.58f, -0.48f, 0f), (1f, 0.24f, 0.52f)),
            new((-0.58f, -0.48f, 0f), (0.08f, 0.92f, 1f)),
            new((0f, 0.58f, 0f), (1f, 0.92f, 0.18f)),
        ];

        vao = gl.GenVertexArray();
        gl.BindVertexArray(vao);
        gl.BindBuffer(GlBufferTarget.ArrayBuffer, gl.GenBuffer());
        gl.BufferData(GlBufferTarget.ArrayBuffer, vertices.AsSpan(), GlBufferUsage.StaticDraw);
        positionColorProgram.SetAttributes();
        gl.UnbindBuffer(GlBufferTarget.ArrayBuffer);
        gl.UnbindVertexArray();

        method = typeof(LivePatchTarget).GetMethod(
            nameof(LivePatchTarget.SceneMode),
            BindingFlags.Public | BindingFlags.Static)!;
        profiler = InterceptionProfiler.Connect();

        input.Track = true;
        Console.WriteLine("ALVORKIT ENGINE LIVE PATCH DEMO");
        Console.WriteLine($"CAPABILITIES {JsonSerializer.Serialize(profiler.Capabilities)}");
        Console.WriteLine("Space installs/replaces SceneMode. R removes it and restores original IL.");
        UpdateTitle();
        screen.IsVisible = true;
    }

    /// <inheritdoc />
    public override void Update(double delta)
    {
        _ = delta;
        if (patch is not null &&
            patch.LastRequestId != observedRequest)
        {
            Observe(patch.LastRequestId);
        }
        else if (observedRequest != 0 && !completion.IsTerminal)
        {
            Observe(observedRequest);
        }

        if (keyboard.IsKeyPressed(Keys.Space))
        {
            if (patch is null || completion.State == InterceptionState.Removed)
            {
                patch = profiler.Install(
                    LivePatchProof.ConstantInt32Plan(method, nextMode));
                Console.WriteLine(
                    $"SEND install {{ patch: {patch.PatchId}, request: {patch.LastRequestId}, value: {nextMode} }}");
            }
            else if (completion.State == InterceptionState.Active)
            {
                var request = patch.Replace(
                    LivePatchProof.ConstantInt32Plan(method, nextMode));
                Console.WriteLine(
                    $"SEND replace {{ patch: {patch.PatchId}, request: {request}, value: {nextMode} }}");
            }

            nextMode = nextMode == 1 ? 2 : 1;
        }

        if (keyboard.IsKeyPressed(Keys.R) &&
            patch is not null &&
            completion.State == InterceptionState.Active)
        {
            var request = patch.Remove();
            Console.WriteLine($"SEND remove {{ patch: {patch.PatchId}, request: {request} }}");
        }
    }

    /// <inheritdoc />
    public override void Render()
    {
        var mode = LivePatchTarget.SceneMode();
        backbuffer.Clear(mode switch
        {
            1 => (0.28f, 0.008f, 0.13f, 1f),
            2 => (0.015f, 0.24f, 0.12f, 1f),
            _ => (0.012f, 0.025f, 0.09f, 1f)
        });
        gl.Viewport(canvas.Size);
        gl.UseProgram(positionColorProgram.Id);
        gl.BindVertexArray(vao);
        gl.DrawArrays(GlPrimitiveType.Triangles, 0, 3);
        gl.UnbindVertexArray();
        gl.UnuseProgram();
        gl.ResetViewport();
    }

    private void Observe(ulong requestId)
    {
        var current = profiler.GetCompletion(requestId);
        if (current == completion)
            return;

        completion = current;
        observedRequest = requestId;
        LivePatchProof.PrintCompletion("RECEIVE", completion);
        UpdateTitle();
    }

    private void UpdateTitle() =>
        screen.Title = completion.State switch
        {
            InterceptionState.Active =>
                "AlvorKit Live Patch — ACTIVE — Space replaces, R restores",
            InterceptionState.Failed =>
                $"AlvorKit Live Patch — FAILED 0x{completion.HResult:X8}",
            InterceptionState.Applying or InterceptionState.Requested or InterceptionState.Queued =>
                "AlvorKit Live Patch — REJIT IN PROGRESS",
            InterceptionState.Removing =>
                "AlvorKit Live Patch — RESTORING ORIGINAL",
            _ =>
                "AlvorKit Live Patch — ORIGINAL — Space installs a patch"
        };
}
