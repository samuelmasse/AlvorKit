namespace AlvorKit;

[Components]
public interface ICoreBenchComponents
{
    int SparseFirst { get; set; }
    int SparseSecond { get; set; }
    int SparseThird { get; set; }
    [Archetypal] int First { get; set; }
    [Archetypal] int Second { get; set; }
    [Archetypal] int Third { get; set; }
}
