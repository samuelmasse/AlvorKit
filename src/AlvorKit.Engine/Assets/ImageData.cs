namespace AlvorKit.Engine;

/// <summary>Decoded RGBA image pixels and their dimensions.</summary>
public sealed record class ImageData(Vec2u Size, ReadOnlyMemory<(byte Red, byte Green, byte Blue, byte Alpha)> Pixels);
