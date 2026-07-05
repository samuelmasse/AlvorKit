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
    private string[] modelStatLabels = [];
    private string[] modelStatValues = [];
    private string[] animationLabels = [];
    private string[] animationDurationLabels = [];
    private bool cameraCaptured;

    public bool CameraCaptured => cameraCaptured;
    public int ModelStatCount => modelStatLabels.Length;
    public int AnimationLineCount => animationLabels.Length;
    public int SelectedAnimationIndex => Model.SelectedAnimationIndex;
    public string ActiveAnimationName => Model.GetAnimationName(Model.SelectedAnimationIndex);

    public void Load()
    {
        model = GlbModel.Load(gl);
        (modelStatLabels, modelStatValues) = CreateModelStats(model);
        (animationLabels, animationDurationLabels) = CreateAnimationLabels(model);
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

    public string ModelStatLabelAt(int index) => modelStatLabels[index];

    public string ModelStatValueAt(int index) => modelStatValues[index];

    public string AnimationLabelAt(int index) => animationLabels[index];

    public string AnimationDurationLabelAt(int index) => animationDurationLabels[index];

    public void Dispose()
    {
        ReleaseCamera();
        model?.Dispose();
        model = null;
    }

    private GlbModel Model => model ?? throw new InvalidOperationException("The GLB model has not been loaded.");

    private static (string[] Labels, string[] Values) CreateModelStats(GlbModel model) =>
    (
        ["Model", "Vertices", "Triangles", "Joints", "Texture"],
        [
            "azure_tentacle_monster_tex256.glb",
            string.Format(CultureInfo.InvariantCulture, "{0}", model.VertexCount),
            string.Format(CultureInfo.InvariantCulture, "{0}", model.TriangleCount),
            string.Format(CultureInfo.InvariantCulture, "{0}", model.JointCount),
            string.Format(CultureInfo.InvariantCulture, "{0} x {1} nearest", model.TextureWidth, model.TextureHeight),
        ]
    );

    private static (string[] Labels, string[] Durations) CreateAnimationLabels(GlbModel model)
    {
        var labels = new string[model.AnimationCount];
        var durations = new string[model.AnimationCount];
        for (var index = 0; index < labels.Length; index++)
        {
            var duration = model.GetAnimationDuration(index);
            labels[index] = string.Format(CultureInfo.InvariantCulture, "{0}. {1}", index + 1, model.GetAnimationName(index));
            durations[index] = duration <= 0f
                ? "none"
                : string.Format(CultureInfo.InvariantCulture, "{0:0.###}s", duration);
        }

        return (labels, durations);
    }
}
