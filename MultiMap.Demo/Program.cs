// See https://aka.ms/new-console-template for more information
using MultiMap.Helpers;
using MultiMap.Interfaces;

ISimpleMultiMap<string, int> multiMap1 = MultiMap.Demo.TestDataHelper.CreateSampleMultiMap1();
ISimpleMultiMap<string, int> multiMap2 = MultiMap.Demo.TestDataHelper.CreateSampleMultiMap2();

MultiMap.Demo.TestDataHelper.PrintMultiMap("MULTI MAP 1", multiMap1);
MultiMap.Demo.TestDataHelper.PrintMultiMap("MULTI MAP 2", multiMap2);

multiMap1 = MultiMap.Demo.TestDataHelper.CreateSampleMultiMap1();
multiMap1 = multiMap1.Union(multiMap2);
MultiMap.Demo.TestDataHelper.PrintMultiMap("UNION", multiMap1);

multiMap1 = MultiMap.Demo.TestDataHelper.CreateSampleMultiMap1();
multiMap1 = multiMap1.Intersect(multiMap2);
MultiMap.Demo.TestDataHelper.PrintMultiMap("INTERSECT", multiMap1);

multiMap1 = MultiMap.Demo.TestDataHelper.CreateSampleMultiMap1();
multiMap1 = multiMap1.ExceptWith(multiMap2);
MultiMap.Demo.TestDataHelper.PrintMultiMap("EXCEPT WITH 1", multiMap1);

multiMap1 = MultiMap.Demo.TestDataHelper.CreateSampleMultiMap1();
multiMap1 = multiMap2.ExceptWith(multiMap1);
MultiMap.Demo.TestDataHelper.PrintMultiMap("EXCEPT WITH 2", multiMap1);

multiMap1 = MultiMap.Demo.TestDataHelper.CreateSampleMultiMap1();
multiMap2 = MultiMap.Demo.TestDataHelper.CreateSampleMultiMap2();
multiMap1 = multiMap1.SymmetricExceptWith(multiMap2);
MultiMap.Demo.TestDataHelper.PrintMultiMap("SYMMETRIC EXCEPT WITH", multiMap1);

Console.ReadLine();
