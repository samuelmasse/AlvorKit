namespace AlvorKit;

/// <summary>Provides shared Blend controls with root-owned fonts and control textures.</summary>
[Root]
public class RootBlendUi(RootUi ui, RootInter fonts, RootGl gl) : BlendUi(ui, fonts, gl);
