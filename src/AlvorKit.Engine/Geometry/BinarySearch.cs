namespace AlvorKit.Engine;

/// <summary>Binary-search helpers for sorted numeric lists.</summary>
public static class BinarySearch
{
    /// <summary>Finds the inclusive index range whose values fall between <paramref name="inclusiveMin"/> and <paramref name="inclusiveMax"/>.</summary>
    public static (int Start, int End, int Count) FindRange<T>(IList<T> list, T inclusiveMin, T inclusiveMax) where T : INumber<T>
    {
        if (list.Count == 0)
            return (-1, -1, 0);

        var start = FirstGreaterOrEqual(list, inclusiveMin);
        var end = LastLessOrEqual(list, inclusiveMax);
        return start == -1 || end == -1 || start > end ? (-1, -1, 0) : (start, end, end - start + 1);
    }

    /// <summary>Returns the first index whose value is greater than or equal to <paramref name="value"/>, or <c>-1</c>.</summary>
    public static int FirstGreaterOrEqual<T>(IList<T> list, T value) where T : INumber<T> =>
        Search(list, value, static (item, value) => item >= value, moveRightOnMatch: false);

    /// <summary>Returns the last index whose value is less than or equal to <paramref name="value"/>, or <c>-1</c>.</summary>
    public static int LastLessOrEqual<T>(IList<T> list, T value) where T : INumber<T> =>
        Search(list, value, static (item, value) => item <= value, moveRightOnMatch: true);

    /// <summary>Returns the first index whose value is strictly greater than <paramref name="value"/>, or <c>-1</c>.</summary>
    public static int SmallestStrictlyLarger<T>(IList<T> list, T value) where T : INumber<T> =>
        Search(list, value, static (item, value) => item > value, moveRightOnMatch: false);

    /// <summary>Returns the last index whose value is strictly smaller than <paramref name="value"/>, or <c>-1</c>.</summary>
    public static int LargestStrictlySmaller<T>(IList<T> list, T value) where T : INumber<T> =>
        Search(list, value, static (item, value) => item < value, moveRightOnMatch: true);

    private static int Search<T>(IList<T> list, T value, Func<T, T, bool> match, bool moveRightOnMatch) where T : INumber<T>
    {
        var left = 0;
        var right = list.Count - 1;
        var result = -1;
        while (left <= right)
        {
            var mid = left + (right - left) / 2;
            if (match(list[mid], value))
            {
                result = mid;
                if (moveRightOnMatch)
                    left = mid + 1;
                else
                    right = mid - 1;
            }
            else if (moveRightOnMatch)
                right = mid - 1;
            else
                left = mid + 1;
        }

        return result;
    }
}
