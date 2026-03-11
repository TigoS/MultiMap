// See https://aka.ms/new-console-template for more information
using MultiMap.Entities;

var multiMap = new MultiMapList<string, int>();

multiMap.Add("A", 1);
multiMap.Add("A", 2);
multiMap.Add("B", 3);

foreach (var value in multiMap.Get("A"))
{
    Console.WriteLine(value);
}