namespace AlvorKit.Mocking.Test;

public class DerivedMock : BaseMock
{
    public override int this[string key] { get => 353; set { } }
    public override int GetValue() => 232;
    public override sealed int ComputeSum(int a, int b) => a * a;
    public override int Property => base.Property;
}
