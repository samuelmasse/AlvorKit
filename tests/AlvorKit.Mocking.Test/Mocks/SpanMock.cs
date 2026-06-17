namespace AlvorKit.Mocking.Test;

public class SpanMock
{
    public virtual Span<int> Prop => default;
    public virtual Span<int> Method() => default;
    public virtual int Get(Span<int> arg) => 34;
}
