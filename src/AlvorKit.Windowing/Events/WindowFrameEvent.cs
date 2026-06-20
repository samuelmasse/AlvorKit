namespace AlvorKit.Windowing;

/// <summary>Frame timing data from the host event loop.</summary>
/// <param name="Time">Seconds elapsed since the previous frame.</param>
/// <param name="TotalTime">Seconds elapsed since the window time source started.</param>
public readonly record struct WindowFrameEvent(double Time, double TotalTime = 0);
