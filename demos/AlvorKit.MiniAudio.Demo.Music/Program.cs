const string TrackStem = "la-espero-demo";

unsafe
{
    var audioRoot = Path.Combine(ProjectRoot.ResDirectory(typeof(MusicDemoMarker)), "audio");

    Console.WriteLine("AlvorKit.MiniAudio.Demo.Music - WAV, MP3, and FLAC playback");
    Console.WriteLine($"Audio: {audioRoot}");
    Console.WriteLine();

    var ma = new MaBackend();
    var engine = AllocateNativeObject<MaEngine>();
    MaSound* currentSound = null;

    // Keep one engine alive while swapping sound objects so the console can compare formats without restarting the process.
    Require(ma, "ma_engine_init", ma.EngineInit(null, engine));

    try
    {
        while (true)
        {
            Console.Write("Play wav, mp3, flac, or q to quit: ");
            var choice = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (choice is "q" or "quit" or "exit")
                break;

            if (choice is not ("wav" or "mp3" or "flac"))
            {
                Console.WriteLine("Choose wav, mp3, flac, or q.");
                continue;
            }

            var path = Path.Combine(audioRoot, $"{TrackStem}.{choice}");
            if (!File.Exists(path))
                throw new FileNotFoundException("Required demo audio file is missing.", path);

            StopCurrentSound(ma, ref currentSound);
            currentSound = LoadLoopingMusic(ma, engine, path);

            Console.WriteLine($"Playing {Path.GetFileName(path)}. Enter another format to switch.");
            Console.WriteLine();
        }
    }
    finally
    {
        StopCurrentSound(ma, ref currentSound);
        ma.EngineUninit(engine);
        NativeMemory.Free(engine);
    }
}

return 0;

// Allocates native storage for miniaudio's stateful C objects.
static unsafe T* AllocateNativeObject<T>()
    where T : unmanaged =>
    (T*)NativeMemory.AllocZeroed((nuint)sizeof(T));

// Creates and starts one looping music sound from a file path.
static unsafe MaSound* LoadLoopingMusic(Ma ma, MaEngine* engine, string path)
{
    var sound = AllocateNativeObject<MaSound>();
    const MaSoundFlags flags =
        MaSoundFlags.SoundFlagStream |
        MaSoundFlags.SoundFlagLooping |
        MaSoundFlags.SoundFlagNoSpatialization;

    var initResult = ma.SoundInitFromFile(engine, path, (uint)flags, null, null, sound);
    if (initResult != MaResult.Success)
    {
        NativeMemory.Free(sound);
        Require(ma, "ma_sound_init_from_file", initResult);
    }

    var startResult = ma.SoundStart(sound);
    if (startResult != MaResult.Success)
    {
        ma.SoundUninit(sound);
        NativeMemory.Free(sound);
        Require(ma, "ma_sound_start", startResult);
    }

    return sound;
}

// Stops and releases the current sound when switching formats or ending the demo.
static unsafe void StopCurrentSound(Ma ma, ref MaSound* currentSound)
{
    if (currentSound == null)
        return;

    _ = ma.SoundStop(currentSound);
    ma.SoundUninit(currentSound);
    NativeMemory.Free(currentSound);
    currentSound = null;
}

// Turns miniaudio result codes into readable demo failures.
static void Require(Ma ma, string operation, MaResult result)
{
    if (result == MaResult.Success)
        return;

    ma.ResultDescription(result, out var description);
    throw new InvalidOperationException($"{operation} failed: {description ?? result.ToString()}");
}

/// <summary>Assembly marker used to resolve repository demo assets.</summary>
internal sealed class MusicDemoMarker;
