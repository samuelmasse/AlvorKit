namespace AlvorKit;

[Bench]
public class HashMeasurements
{
    private ulong retained;

    public BenchResult TableInt32Inline(int mask)
    {
        var keys = new int[4096];

        for (var index = 0; index < keys.Length; index++)
            keys[index] = (index * 7919) - 1_000_003;

        var timer = BenchTimer.Start();
        var sum = AlvorKit.TableInt32Inline.Run(keys, mask, 256);
        var result = timer.Stop(keys.Length * 256, "key");
        retained = sum;
        return result;
    }

    public BenchResult TableInt32Helper(int mask)
    {
        var keys = new int[4096];

        for (var index = 0; index < keys.Length; index++)
            keys[index] = (index * 7919) - 1_000_003;

        var timer = BenchTimer.Start();
        var sum = AlvorKit.TableInt32Helper.Run(keys, mask, 256);
        var result = timer.Stop(keys.Length * 256, "key");
        retained = sum;
        return result;
    }

    public BenchResult TableInt32PairInline(int mask)
    {
        var keys = new int[4096];
        var second = new int[keys.Length];

        for (var index = 0; index < keys.Length; index++)
        {
            keys[index] = (index * 7919) - 1_000_003;
            second[index] = (index * 104729) + 17;
        }

        var timer = BenchTimer.Start();
        var sum = AlvorKit.TableInt32PairInline.Run(keys, second, mask, 256);
        var result = timer.Stop(keys.Length * 256, "key");
        retained = sum;
        return result;
    }

    public BenchResult TableInt32PairHelper(int mask)
    {
        var keys = new int[4096];
        var second = new int[keys.Length];

        for (var index = 0; index < keys.Length; index++)
        {
            keys[index] = (index * 7919) - 1_000_003;
            second[index] = (index * 104729) + 17;
        }

        var timer = BenchTimer.Start();
        var sum = AlvorKit.TableInt32PairHelper.Run(keys, second, mask, 256);
        var result = timer.Stop(keys.Length * 256, "key");
        retained = sum;
        return result;
    }

    public BenchResult TableInt64Inline(int mask)
    {
        var keys = new long[4096];

        for (var index = 0; index < keys.Length; index++)
            keys[index] = ((long)((index * 7919) - 1_000_003) << 32) | (uint)((index * 104729) + 17);

        var timer = BenchTimer.Start();
        var sum = AlvorKit.TableInt64Inline.Run(keys, mask, 256);
        var result = timer.Stop(keys.Length * 256, "key");
        retained = sum;
        return result;
    }

    public BenchResult TableInt64Helper(int mask)
    {
        var keys = new long[4096];

        for (var index = 0; index < keys.Length; index++)
            keys[index] = ((long)((index * 7919) - 1_000_003) << 32) | (uint)((index * 104729) + 17);

        var timer = BenchTimer.Start();
        var sum = AlvorKit.TableInt64Helper.Run(keys, mask, 256);
        var result = timer.Stop(keys.Length * 256, "key");
        retained = sum;
        return result;
    }

    public BenchResult TableUInt64Inline(int mask)
    {
        var keys = new ulong[4096];

        for (var index = 0; index < keys.Length; index++)
            keys[index] = Unsafe.BitCast<long, ulong>(((long)((index * 7919) - 1_000_003) << 32) | (uint)((index * 104729) + 17));

        var timer = BenchTimer.Start();
        var sum = AlvorKit.TableUInt64Inline.Run(keys, mask, 256);
        var result = timer.Stop(keys.Length * 256, "key");
        retained = sum;
        return result;
    }

    public BenchResult TableUInt64Helper(int mask)
    {
        var keys = new ulong[4096];

        for (var index = 0; index < keys.Length; index++)
            keys[index] = Unsafe.BitCast<long, ulong>(((long)((index * 7919) - 1_000_003) << 32) | (uint)((index * 104729) + 17));

        var timer = BenchTimer.Start();
        var sum = AlvorKit.TableUInt64Helper.Run(keys, mask, 256);
        var result = timer.Stop(keys.Length * 256, "key");
        retained = sum;
        return result;
    }

    public BenchResult TableUInt64Int32Inline(int mask)
    {
        var keys = new ulong[4096];
        var second = new int[keys.Length];

        for (var index = 0; index < keys.Length; index++)
        {
            keys[index] = Unsafe.BitCast<long, ulong>(((long)((index * 7919) - 1_000_003) << 32) | (uint)((index * 104729) + 17));
            second[index] = (index * 104729) + 17;
        }

        var timer = BenchTimer.Start();
        var sum = AlvorKit.TableUInt64Int32Inline.Run(keys, second, mask, 256);
        var result = timer.Stop(keys.Length * 256, "key");
        retained = sum;
        return result;
    }

    public BenchResult TableUInt64Int32Helper(int mask)
    {
        var keys = new ulong[4096];
        var second = new int[keys.Length];

        for (var index = 0; index < keys.Length; index++)
        {
            keys[index] = Unsafe.BitCast<long, ulong>(((long)((index * 7919) - 1_000_003) << 32) | (uint)((index * 104729) + 17));
            second[index] = (index * 104729) + 17;
        }

        var timer = BenchTimer.Start();
        var sum = AlvorKit.TableUInt64Int32Helper.Run(keys, second, mask, 256);
        var result = timer.Stop(keys.Length * 256, "key");
        retained = sum;
        return result;
    }

    public BenchResult Epoch32Dictionary(int count)
    {
        var keys = new int[count];
        var index = new Dictionary<int, int>(count);

        for (var i = 0; i < count; i++)
            keys[i] = (i * 7919) - 1_000_003;
        var passes = 1048576 / count;
        var timer = BenchTimer.Start();
        var sum = AlvorKit.Epoch32Dictionary.Run(index, keys, passes);
        var result = timer.Stop(passes, "clear/fill/read cycle");
        retained = (ulong)sum;
        return result;
    }

    public BenchResult Epoch32EpochIndex(int count)
    {
        var keys = new int[count];
        var index = new EpochIndex32(count);

        for (var i = 0; i < count; i++)
            keys[i] = (i * 7919) - 1_000_003;
        var passes = 1048576 / count;
        var timer = BenchTimer.Start();
        var sum = AlvorKit.Epoch32EpochIndex.Run(index, keys, passes);
        var result = timer.Stop(passes, "clear/fill/read cycle");
        retained = (ulong)sum;
        return result;
    }

    public BenchResult Epoch64Dictionary(int count)
    {
        var keys = new ulong[count];
        var index = new Dictionary<ulong, int>(count);

        for (var i = 0; i < count; i++)
            keys[i] = ((ulong)(uint)(i * 7919) << 32) | (uint)(i * 104729);
        var passes = 1048576 / count;
        var timer = BenchTimer.Start();
        var sum = AlvorKit.Epoch64Dictionary.Run(index, keys, passes);
        var result = timer.Stop(passes, "clear/fill/read cycle");
        retained = (ulong)sum;
        return result;
    }

    public BenchResult Epoch64EpochIndex(int count)
    {
        var keys = new ulong[count];
        var index = new EpochIndex64(count);

        for (var i = 0; i < count; i++)
            keys[i] = ((ulong)(uint)(i * 7919) << 32) | (uint)(i * 104729);
        var passes = 1048576 / count;
        var timer = BenchTimer.Start();
        var sum = AlvorKit.Epoch64EpochIndex.Run(index, keys, passes);
        var result = timer.Stop(passes, "clear/fill/read cycle");
        retained = (ulong)sum;
        return result;
    }

    public BenchResult ChecksumInline()
    {
        var values = new ulong[4096];

        for (var i = 0; i < values.Length; i++)
            values[i] = (ulong)(i * 17);
        var timer = BenchTimer.Start();
        var sum = AlvorKit.ChecksumInline.Run(values, 256);
        var result = timer.Stop(values.Length * 256, "value");
        retained = sum;
        return result;
    }

    public BenchResult ChecksumHelper()
    {
        var values = new ulong[4096];

        for (var i = 0; i < values.Length; i++)
            values[i] = (ulong)(i * 17);
        var timer = BenchTimer.Start();
        var sum = AlvorKit.ChecksumHelper.Run(values, 256);
        var result = timer.Stop(values.Length * 256, "value");
        retained = sum;
        return result;
    }
}
