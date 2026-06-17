namespace AlvorKit.Mocking.Test;

public class ClassMock(string name)
{
    public event Action<string>? Event;

    public string Name => name;
    public string LastName => "Roger";

    public void Invoke() => Event?.Invoke(string.Empty);
    public int ReturnDouble(int arg) => arg * 2;
}
