namespace AlvorKit.Mocking.Test;

public class InParamMock
{
    public virtual int Transform(in int value) => value * 2;
    public virtual void Process(in int x, in int y) { }
    public virtual int Add(int a, in int b) => a + b;
}
