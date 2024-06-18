using System.Collections.Concurrent;

namespace PA.Graphics;

public class ParallelHelper
{
    public static void Foreach<T>(T[] array, Action<T> action, bool isSingleThread = false)
    {
        if (array.Length == 0)
        {
            return;
        }

        if (isSingleThread)
        {
            foreach (T item in array)
            {
                action(item);
            }
        }
        else
        {
            Parallel.ForEach(Partitioner.Create(0, array.Length), (range) =>
            {
                for (int i = range.Item1; i < range.Item2; i++)
                {
                    action(array[i]);
                }
            });
        }
    }

    public static void ProcessingPixels(Pixel[] pixels, Box2d box, Action<Pixel> action, bool isSingleThread = false)
    {
        Pixel[] pixelsInBox = pixels.AsParallel().Where((pixel) => box.Contains(pixel.X, pixel.Y)).ToArray();

        Foreach(pixelsInBox, action, isSingleThread);
    }
}
