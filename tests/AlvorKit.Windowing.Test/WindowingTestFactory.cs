namespace AlvorKit.Windowing.Test;

internal static class WindowingTestFactory
{
    public static (FakeWindowHost Host, WindowLoop Loop) Create(Vec2u? clientSize = null)
    {
        var host = new FakeWindowHost
        {
            ClientSize = clientSize ?? default
        };

        return (host, new WindowLoop(host));
    }
}
