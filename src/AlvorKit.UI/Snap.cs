namespace AlvorKit.UI;

public static class Snap
{
    public static float Round(float value, float snap) => snap > 0 ? (float)Math.Round(value / snap) * snap : value;
    public static float Ceiling(float value, float snap) => snap > 0 ? (float)Math.Ceiling(value / snap) * snap : value;
    public static float Floor(float value, float snap) => snap > 0 ? (float)Math.Floor(value / snap) * snap : value;
}
