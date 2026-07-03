namespace AlvorKit.UI;

[Root]
public class RootUiUpdate
{
    internal void Update(EntMut n)
    {
        n.OnUpdateFV.Resolve()?.Invoke();

        foreach (var c in n.NodesR.Span)
            Update(c);
    }
}
