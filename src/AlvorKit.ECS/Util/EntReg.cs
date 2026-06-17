namespace AlvorKit.ECS;

internal static class EntReg
{
    internal const int PageBits = 12;
    internal const int PageSize = 1 << PageBits;
    internal const int PageMask = PageSize - 1;

    internal static readonly object Lock = new();
    internal static EntRegView View = new();

    internal static List<EntStorageView> Storage = [];
    internal static readonly List<EntField> Fields = [];

    internal static int NextPage = 1;
    internal static readonly ConcurrentBag<int> FreePages = [];

    internal static readonly List<EntAllocator> Allocators = [new(0, true), new(1, false)];
    internal static readonly ConcurrentBag<int> FreeAllocators = [];

    internal static readonly List<int> PageAllocators = [-1];
    internal static int[][] PageGenerations = [[1]];
    internal static readonly EntPageFieldMap PageFields = new();
    internal static readonly EntPageFieldMap PageRefFields = new();
}

internal class EntRegView
{
    internal int PageBits => EntReg.PageBits;
    internal int PageSize => EntReg.PageSize;
    internal int PageMask => EntReg.PageMask;

    internal List<EntStorageView> Storage => EntReg.Storage;
    internal List<EntField> Fields => EntReg.Fields;

    internal int NextPage => EntReg.NextPage;
    internal ConcurrentBag<int> FreePages => EntReg.FreePages;

    internal List<EntAllocator> Allocators => EntReg.Allocators;
    internal ConcurrentBag<int> FreeAllocators => EntReg.FreeAllocators;

    internal List<int> PageAllocators => EntReg.PageAllocators;
    internal int[][] PageGenerations => EntReg.PageGenerations;

    internal EntPageFieldMap PageFields => EntReg.PageFields;
    internal EntPageFieldMap PageRefFields => EntReg.PageRefFields;
}
