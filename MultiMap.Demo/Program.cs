// See https://aka.ms/new-console-template for more information
using MultiMap.Demo;
using MultiMap.Helpers;
using MultiMap.Interfaces;

ISimpleMultiMap<string, int> multiMap1 = TestDataHelper.CreateSampleMultiMap1();
ISimpleMultiMap<string, int> multiMap2 = TestDataHelper.CreateSampleMultiMap2();

TestDataHelper.PrintMultiMap("MULTI MAP 1", multiMap1);
TestDataHelper.PrintMultiMap("MULTI MAP 2", multiMap2);

multiMap1 = TestDataHelper.CreateSampleMultiMap1();
multiMap1 = multiMap1.Union(multiMap2);
TestDataHelper.PrintMultiMap("UNION", multiMap1);

multiMap1 = TestDataHelper.CreateSampleMultiMap1();
multiMap1 = multiMap1.Intersect(multiMap2);
TestDataHelper.PrintMultiMap("INTERSECT", multiMap1);

multiMap1 = TestDataHelper.CreateSampleMultiMap1();
multiMap1 = multiMap1.ExceptWith(multiMap2);
TestDataHelper.PrintMultiMap("EXCEPT WITH 1", multiMap1);

multiMap1 = TestDataHelper.CreateSampleMultiMap1();
multiMap1 = multiMap2.ExceptWith(multiMap1);
TestDataHelper.PrintMultiMap("EXCEPT WITH 2", multiMap1);

multiMap1 = TestDataHelper.CreateSampleMultiMap1();
multiMap2 = TestDataHelper.CreateSampleMultiMap2();
multiMap1 = multiMap1.SymmetricExceptWith(multiMap2);
TestDataHelper.PrintMultiMap("SYMMETRIC EXCEPT WITH", multiMap1);

multiMap1 = TestDataHelper.CreateSampleMultiMap1();
multiMap2 = TestDataHelper.CreateSampleMultiMap1();
multiMap2.Add("D", 4);
bool isSubset = multiMap1.IsSubsetOf(multiMap2);
TestDataHelper.PrintLineBreak();
TestDataHelper.PrintMultiMap("MULTI MAP 1", multiMap1);
TestDataHelper.PrintMultiMap("MULTI MAP 2", multiMap2);
Console.WriteLine($"1 IS SUBSET OF 2: {isSubset}{Environment.NewLine}");

multiMap1 = TestDataHelper.CreateSampleMultiMap1();
multiMap2 = TestDataHelper.CreateSampleMultiMap1();
multiMap2.RemoveKey("B");
bool isSuperset = multiMap1.IsSupersetOf(multiMap2);
TestDataHelper.PrintLineBreak();
TestDataHelper.PrintMultiMap("MULTI MAP 1", multiMap1);
TestDataHelper.PrintMultiMap("MULTI MAP 2", multiMap2);
Console.WriteLine($"1 IS SUPERSET OF 2: {isSuperset}{Environment.NewLine}");

multiMap1 = TestDataHelper.CreateSampleMultiMap1();
multiMap2 = TestDataHelper.CreateSampleMultiMap2();
bool overlaps = multiMap1.Overlaps(multiMap2);
TestDataHelper.PrintLineBreak();
TestDataHelper.PrintMultiMap("MULTI MAP 1", multiMap1);
TestDataHelper.PrintMultiMap("MULTI MAP 2", multiMap2);
Console.WriteLine($"1 OVERLAPS WITH 2: {overlaps}{Environment.NewLine}");

multiMap1 = TestDataHelper.CreateSampleMultiMap1();
multiMap2 = TestDataHelper.CreateSampleMultiMap1();
bool isSetEqual = multiMap1.SetEquals(multiMap2);
TestDataHelper.PrintLineBreak();
TestDataHelper.PrintMultiMap("MULTI MAP 1", multiMap1);
TestDataHelper.PrintMultiMap("MULTI MAP 2", multiMap2);
Console.WriteLine($"1 IS SET EQUAL TO 2: {isSetEqual}{Environment.NewLine}");

Console.ReadLine();
