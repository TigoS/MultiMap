using MultiMap.Entities;
using MultiMap.Interfaces;

namespace MultiMap.Helpers
{
    /// <summary>
    /// Provides factory methods for creating sample <see cref="ISimpleMultiMap{TKey, TValue}"/> instances and printing their contents.
    /// </summary>
    public class TestDataHelper
    {
        /// <summary>
        /// Creates a sample multimap containing keys "A" (values 1, 2) and "B" (value 3).
        /// </summary>
        /// <returns>A pre-populated <see cref="ISimpleMultiMap{TKey, TValue}"/> instance.</returns>
        public static ISimpleMultiMap<string, int> CreateSampleMultiMap1()
        {
            var multiMap = new SimpleMultiMap<string, int>
            {
                { "A", 1 },
                { "A", 2 },
                { "B", 3 }
            };

            return multiMap;
        }

        /// <summary>
        /// Creates a sample multimap containing keys "A" (values 1, 3) and "C" (values 4, 3).
        /// </summary>
        /// <returns>A pre-populated <see cref="ISimpleMultiMap{TKey, TValue}"/> instance.</returns>
        public static ISimpleMultiMap<string, int> CreateSampleMultiMap2()
        {
            var multiMap = new SimpleMultiMap<string, int>
            {
                { "A", 1 },
                { "A", 3 },
                { "C", 4 },
                { "C", 3 },
            };

            return multiMap;
        }

        /// <summary>
        /// Prints all key-value pairs in the specified multimap to the console.
        /// </summary>
        /// <param name="title">A title to display before the multimap contents.</param>
        /// <param name="multiMap">The multimap to print.</param>
        public static void PrintMultiMap(string title, ISimpleMultiMap<string, int> multiMap)
        {
            Console.WriteLine($"{title}:");

            foreach (var item in multiMap.Flatten())
            {
                Console.WriteLine($"{item.Key}: {item.Value}");
            }

            Console.WriteLine();
        }
    }
}
