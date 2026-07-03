namespace AlvorKit.UI;

[Flags]
public enum Alignment
{
    None = 0,
    Horizontal = 1 << 0,
    Vertical = 1 << 1,
    Left = 1 << 2,
    Right = 1 << 3,
    Top = 1 << 4,
    Bottom = 1 << 5,
    Center = Horizontal | Vertical,
}
