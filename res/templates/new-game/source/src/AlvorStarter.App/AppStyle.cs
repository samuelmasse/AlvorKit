namespace AlvorStarter.App;

/// <summary>Blend-backed UI style for Alvor Starter.</summary>
[App]
public class AppStyle(RootInter inter, RootGl gl, RootUiScale scale, RootKeyboard keyboard)
    : BlendStyle(inter, gl, scale, keyboard);
