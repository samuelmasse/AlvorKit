namespace AlvorKit.MiniAudio;

/// <summary>Maps ma_result. Only the common values are named.</summary>
public enum MaResult
{
    Success = 0,
    Error = -1,
    InvalidArgs = -2,
    InvalidOperation = -3,
    OutOfMemory = -4
}
