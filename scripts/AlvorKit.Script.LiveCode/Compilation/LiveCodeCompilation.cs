namespace AlvorKit.Script.LiveCode;

/// <summary>Portable assembly, symbols, and entry type emitted for one valid LiveCode command.</summary>
internal sealed record LiveCodeCompilation(
    byte[] Assembly,
    byte[] Symbols,
    string EntryType);
