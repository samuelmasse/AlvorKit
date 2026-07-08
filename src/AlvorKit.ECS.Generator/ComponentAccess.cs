namespace AlvorKit.ECS.Generator;

internal static class ComponentAccess
{
        internal static string ToAccessString(Accessibility accessibility) => accessibility switch
    {
        Accessibility.Internal => "internal",
        Accessibility.Protected => "protected",
        Accessibility.ProtectedOrInternal => "protected internal",
        Accessibility.Private => "private",
        _ => "public"
    };

        internal static string WiderAccess(string first, string second)
    {
        if (first == second)
            return first;

        var firstRank = AccessRank(first);
        var secondRank = AccessRank(second);

        if (firstRank == 1 && secondRank == 2 || firstRank == 2 && secondRank == 1)
            return "protected internal";

        return firstRank > secondRank ? first : second;
    }

        private static int AccessRank(string access) => access switch
    {
        "private" => 0,
        "protected" => 1,
        "internal" => 2,
        "protected internal" => 3,
        "public" => 4,
        _ => 4
    };
}
