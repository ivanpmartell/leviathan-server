using System;
using System.Threading;

namespace leviathan_server
{
    class Program
    {
        private const int listenPort = 16700;

        static void Main(string[] args)
        {
            Thread t = new Thread(delegate ()
            {
                Server myserver = new Server("127.0.0.1", listenPort);
            });
            t.Start();
            Console.WriteLine("Server listening on {0}...", listenPort);
        }
    }
    public static class Extensions
    {
        /// <summary>
        /// Get the array slice between the two indexes.
        /// ... Inclusive for start index, exclusive for end index.
        /// </summary>
        public static T[] Slice<T>(this T[] source, int start, int end)
        {
            // Handles negative ends.
            if (end < 0)
            {
                end = source.Length + end;
            }
            int len = end - start;

            // Return new array.
            T[] res = new T[len];
            for (int i = 0; i < len; i++)
            {
                res[i] = source[i + start];
            }
            return res;
        }
    }
}