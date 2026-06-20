namespace AlvorKit.Windowing.Test;

internal static class WindowingTestFactory
{
    public static (FakeWindowHost Host, WindowLoop Loop) Create(Vector2? clientSize = null)
    {
        var host = new FakeWindowHost
        {
            ClientSize = clientSize ?? default
        };

        return (host, new WindowLoop(host));
    }
}
