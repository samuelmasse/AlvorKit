namespace AlvorKit;

/// <summary>Registers the default Blend toolkit with root-owned control textures.</summary>
public class BlendUiSetup : IRootSetup
{
    public void Apply(RootScope root) => root.Add(new BlendUi(root.Get<RootBlend>(), root.Get<RootGl>()));
}
