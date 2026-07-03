namespace AlvorKit.Engine;

/// <summary>Decoded RGBA image pixels and their dimensions.</summary>
public record struct ImageData(Vec2i Size, ReadOnlyMemory<Vec4u8> Pixels);
