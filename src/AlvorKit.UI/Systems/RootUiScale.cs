namespace AlvorKit.UI;

[Root]
public class RootUiScale(RootScale rscale)
{
    private float scale = rscale.Scale;

    public float Scale
    {
        get => scale;
        set => scale = value;
    }
}
