namespace AlvorKit.OpenGL.Demo.AzureTentacle;

/// <summary>Owns the loaded GLB model, camera state, and menu command surface for the demo.</summary>
[App]
public class AppSession(
    RootGl gl,
    RootInput input,
    RootKeyboard keyboard,
    RootMouse mouse) : IDisposable
{
    private readonly FlyCamera camera = new((0f, 0.08f, 4f), 0f, 0f);
    private GlbModel? model;
    private string[] modelInfoLines = [];
    private string[] animationLines = [];
    private bool cameraCaptured;

    public bool CameraCaptured => cameraCaptured;
    public int ModelInfoLineCount => modelInfoLines.Length;
    public int AnimationLineCount => animationLines.Length;
    public int SelectedAnimationIndex => Model.SelectedAnimationIndex;
    public string ActiveAnimationName => Model.GetAnimationName(Model.SelectedAnimationIndex);

    public void Load()
    {
        model = GlbModel.Load(gl);
        modelInfoLines = CreateModelInfoLines(model);
        animationLines = CreateAnimationLines(model);
        CaptureCamera();
    }

    public void Update(double delta)
    {
        var elapsedSeconds = (float)delta;
        Model.Update(elapsedSeconds);
        camera.Update(keyboard, mouse, elapsedSeconds, cameraCaptured);
    }

    public void RenderModel(int framebufferWidth, int framebufferHeight) =>
        Model.Render(framebufferWidth, framebufferHeight, camera.CreateViewMatrix());

    public void CaptureCamera()
    {
        if (cameraCaptured)
            return;

        input.Track = true;
        input.CursorMode = CursorMode.Disabled;
        camera.ResetMouseTracking();
        cameraCaptured = true;
    }

    public void ReleaseCamera()
    {
        if (!cameraCaptured)
            return;

        input.Track = false;
        input.CursorMode = CursorMode.Normal;
        camera.ResetMouseTracking();
        cameraCaptured = false;
    }

    public void SelectNextAnimation() => Model.SelectNextAnimation();

    public void SelectPreviousAnimation() => Model.SelectPreviousAnimation();

    public void SelectAnimation(int animationIndex) => Model.SelectAnimation(animationIndex);

    public string ModelInfoLineAt(int index) => modelInfoLines[index];

    public string AnimationLineAt(int index) => animationLines[index];

    public void Dispose()
    {
        ReleaseCamera();
        model?.Dispose();
        model = null;
    }

    private GlbModel Model => model ?? throw new InvalidOperationException("The GLB model has not been loaded.");

    private static string[] CreateModelInfoLines(GlbModel model) =>
    [
        "Model: azure_tentacle_monster_tex256.glb",
        string.Format(CultureInfo.InvariantCulture, "Vertices: {0}", model.VertexCount),
        string.Format(CultureInfo.InvariantCulture, "Triangles: {0}", model.TriangleCount),
        string.Format(CultureInfo.InvariantCulture, "Texture: {0} x {1} nearest", model.TextureWidth, model.TextureHeight),
        string.Format(CultureInfo.InvariantCulture, "Joints: {0}", model.JointCount),
        string.Format(CultureInfo.InvariantCulture, "Animation slots: {0}", model.AnimationCount),
    ];

    private static string[] CreateAnimationLines(GlbModel model)
    {
        var lines = new string[model.AnimationCount];
        for (var index = 0; index < lines.Length; index++)
        {
            var duration = model.GetAnimationDuration(index);
            lines[index] = duration <= 0f
                ? string.Format(CultureInfo.InvariantCulture, "{0}. {1} (no animation)", index + 1, model.GetAnimationName(index))
                : string.Format(CultureInfo.InvariantCulture, "{0}. {1} ({2:0.###}s)", index + 1, model.GetAnimationName(index), duration);
        }

        return lines;
    }
}
