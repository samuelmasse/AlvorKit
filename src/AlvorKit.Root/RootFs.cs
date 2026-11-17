namespace AlvorKit;

[Root]
public class RootFs
{
    public virtual bool Exists(string path) => File.Exists(path);

    public virtual Stream OpenRead(string path) => File.OpenRead(path);

    public virtual string ReadAllText(string path) => File.ReadAllText(path);

    public virtual byte[] ReadAllBytes(string path) => File.ReadAllBytes(path);
}
