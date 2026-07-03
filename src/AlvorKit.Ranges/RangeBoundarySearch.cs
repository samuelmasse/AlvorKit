namespace AlvorKit.Ranges;

/// <summary>Provides boundary searches over sorted numeric lists used by range indexes.</summary>
internal static class RangeBoundarySearch
{
    /// <summary>Returns the first index whose value is greater than or equal to <paramref name="value"/>.</summary>
    public static int FirstGreaterOrEqual<T>(IList<T> list, T value) where T : INumber<T>
    {
        if (list.Count == 0)
            return -1;

        var left = 0;
        var right = list.Count - 1;
        var result = -1;
        while (left <= right)
        {
            var mid = left + (right - left) / 2;

            if (list[mid] >= value)
            {
                result = mid;
                right = mid - 1;
            }
            else
                left = mid + 1;
        }

        return result;
    }

    /// <summary>Returns the first index whose value is greater than <paramref name="value"/>.</summary>
    public static int SmallestStrictlyLarger<T>(IList<T> list, T value) where T : INumber<T>
    {
        if (list.Count == 0)
            return -1;

        var left = 0;
        var right = list.Count - 1;
        var result = -1;

        while (left <= right)
        {
            var mid = left + (right - left) / 2;

            if (list[mid] > value)
            {
                result = mid;
                right = mid - 1;
            }
            else
                left = mid + 1;
        }

        return result;
    }

    /// <summary>Returns the last index whose value is less than <paramref name="value"/>.</summary>
    public static int LargestStrictlySmaller<T>(IList<T> list, T value) where T : INumber<T>
    {
        if (list.Count == 0)
            return -1;

        var left = 0;
        var right = list.Count - 1;
        var result = -1;

        while (left <= right)
        {
            var mid = left + (right - left) / 2;

            if (list[mid] < value)
            {
                result = mid;
                left = mid + 1;
            }
            else
                right = mid - 1;
        }

        return result;
    }
}
