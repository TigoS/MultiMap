using MultiMap.Entities;
using MultiMap.Interfaces;

namespace MultiMap.Helpers
{
    public class TestDataHelper
    {
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
