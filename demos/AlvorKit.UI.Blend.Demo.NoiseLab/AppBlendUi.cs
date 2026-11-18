namespace AlvorKit;

/// <summary>Uses 12-pixel chip text so decimal points remain visible in value readouts.</summary>
[App]
public class AppBlendUi(RootUi ui, RootInter fonts, RootGl gl)
    : BlendUi(ui, fonts, gl, BlendPalette.Default, new BlendMetrics { ChipFontSize = 12 });
