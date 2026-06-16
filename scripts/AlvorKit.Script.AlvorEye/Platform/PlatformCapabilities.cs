namespace AlvorKit.Script.AlvorEye;

/// <summary>Describes which automation features a platform adapter supports.</summary>
internal sealed class PlatformCapabilities
{
    /// <summary>Whether the adapter can discover desktop windows.</summary>
    public bool WindowDiscovery { get; init; }

    /// <summary>Whether the adapter can move and resize windows.</summary>
    public bool WindowPlacement { get; init; }

    /// <summary>Whether the adapter can capture frames.</summary>
    public bool Capture { get; init; }

    /// <summary>Whether the adapter can send keyboard events.</summary>
    public bool Keyboard { get; init; }

    /// <summary>Whether the adapter can send mouse events.</summary>
    public bool Mouse { get; init; }

    /// <summary>Whether the adapter can freeze and resume processes.</summary>
    public bool ProcessFreeze { get; init; }
}
