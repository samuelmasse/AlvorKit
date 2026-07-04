namespace AlvorKit.OpenGL.Demo.AzureTentacle;

[App]
public class AppModelInfoMenu(
    AppStyle style,
    AppSession session)
{
    public void Create(EntMut root)
    {
        root.Mutate(style.Panel);

        Node(root)
            .Mutate(style.Title)
            .TextV("Model");

        for (var index = 0; index < session.ModelInfoLineCount; index++)
        {
            var line = session.ModelInfoLineAt(index);
            Node(root)
                .Mutate(style.MutedLabel)
                .TextV(line);
        }
    }
}
