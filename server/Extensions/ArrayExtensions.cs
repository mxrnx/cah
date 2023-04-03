namespace Server.Extensions;

public static class ArrayExtensions
{
    /// <summary>
    /// Use the Knuth Shuffle to shuffle array in-place in O(n).
    /// </summary>
    /// <param name="array">Input array</param>
    /// <typeparam name="T">Type of the input array's elements</typeparam>
    public static void Shuffle<T>(this T[] array)
    {
        var random = new Random();
        var max = array.Length;

        while (max > 1)
        {
            var destination = random.Next(max--);
            (array[max], array[destination]) = (array[destination], array[max]);
        }
    }
}
