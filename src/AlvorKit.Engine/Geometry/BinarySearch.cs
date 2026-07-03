namespace AlvorKit.Engine;

public static class BinarySearch
{
    public static (int Start, int End, int Count) FindRange<T>(IList<T> list, T inclusiveMin, T inclusiveMax) where T : INumber<T>
    {
        if (list.Count == 0)
            return (-1, -1, 0);

        var start = FirstGreaterOrEqual(list, inclusiveMin);
        var end = LastLessOrEqual(list, inclusiveMax);
        return start == -1 || end == -1 || start > end ? (-1, -1, 0) : (start, end, end - start + 1);
    }

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

    public static int LastLessOrEqual<T>(IList<T> list, T value) where T : INumber<T>
    {
        if (list.Count == 0)
            return -1;

        var left = 0;
        var right = list.Count - 1;
        var result = -1;

        while (left <= right)
        {
            var mid = left + (right - left) / 2;

            if (list[mid] <= value)
            {
                result = mid;
                left = mid + 1;
            }
            else
                right = mid - 1;
        }

        return result;
    }

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
