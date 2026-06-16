namespace AlvorKit.Script.AlvorEye;

/// <summary>Summary of simple checks performed on one captured frame.</summary>
/// <param name="Width">Frame width in pixels.</param>
/// <param name="Height">Frame height in pixels.</param>
/// <param name="NonBlank">Whether the frame contains enough color variance to avoid being considered blank.</param>
/// <param name="ChangedPixels">Number of pixels different from the baseline color or comparison frame.</param>
/// <param name="ColorHits">Number of pixels close to the requested color.</param>
/// <param name="MinX">Minimum changed pixel x coordinate, or frame width when there are no changed pixels.</param>
/// <param name="MinY">Minimum changed pixel y coordinate, or frame height when there are no changed pixels.</param>
/// <param name="MaxX">Maximum changed pixel x coordinate, or -1 when there are no changed pixels.</param>
/// <param name="MaxY">Maximum changed pixel y coordinate, or -1 when there are no changed pixels.</param>
internal sealed record BasicImageAnalysisResult(
    int Width,
    int Height,
    bool NonBlank,
    int ChangedPixels,
    int ColorHits,
    int MinX,
    int MinY,
    int MaxX,
    int MaxY);
