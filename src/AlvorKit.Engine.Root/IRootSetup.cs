namespace AlvorKit;

/// <summary>Configures a root scope before the boot state is constructed.</summary>
public interface IRootSetup
{
    void Apply(RootScope root);
}
