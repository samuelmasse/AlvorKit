namespace AlvorKit;

/// <summary>Preserves every original receiver-free opcode behind exact delegates.</summary>
internal static class ProfiledReceiverFreeOriginal
{
    internal static int Transform(int value) =>
        ProfiledReceiverFreeTarget.Transform(value);

    internal static string Identity(string value) =>
        ProfiledReceiverFreeTarget.Identity(value);

    internal static void SetStaticNumber(int value) =>
        ProfiledReceiverFreeTarget.StaticNumber = value;

    internal static int GetStaticNumber() =>
        ProfiledReceiverFreeTarget.StaticNumber;

    internal static void WriteStaticField(int value) =>
        ProfiledReceiverFreeTarget.StaticField = value;

    internal static int ReadStaticField() =>
        ProfiledReceiverFreeTarget.StaticField;

    internal static ProfiledReceiverFreeTarget Construct(int value) =>
        new(value);

    internal static int ReadInstanceField(
        ProfiledReceiverFreeTarget target) =>
        target.InstanceField;

    internal static void WriteInstanceField(
        ProfiledReceiverFreeTarget target,
        int value) =>
        target.InstanceField = value;

    internal static string? ReadInstanceReferenceField(
        ProfiledReceiverFreeTarget target) =>
        target.InstanceReferenceField;

    internal static void WriteInstanceReferenceField(
        ProfiledReceiverFreeTarget target,
        string? value) =>
        target.InstanceReferenceField = value;
}
