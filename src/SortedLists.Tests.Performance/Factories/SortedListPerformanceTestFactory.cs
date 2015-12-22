namespace SortedLists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnitBenchmarker;
    using NUnitBenchmarker.Configuration;

    public class SortedListPerformanceTestFactory
    {
        private const int H = 100;
        private const int K = 1000;

        public IEnumerable<Type> ImplementationTypes
        {
            get
            {
                return new[]{
                    //typeof (SortedList<>),
                    //typeof (SortedArrayList<>),
                    //typeof (SortedArray<>),
                    //typeof (SortedGapBuffer<>),
                    typeof (SortedSet<>),
                    typeof (RedBlackTree<>),
                    //typeof (DoublyLinkedRedBlackBinarySearchTree<>),
                    typeof (BTree<>),
                    typeof (SortedSplitList<>),
                    typeof (SortedSplitNodeList<>),
                    //typeof (RedBlackTreeSortedSplitList<>),
                    //typeof (AvlTreeSortedSplitList<>),
                    //typeof (SortedSplitGapBuffer<>),
                };
            }
        }

        private static IEnumerable<int> Sizes
        {
            get
            {
                //return new[] { 1 << 5, 1 << 6, 1 << 7, 1 << 8, 1 << 9, 1 << 10, 1 << 11 };
                //return new[] { 5 * 1, 100 * 1, 200 * 1, 400 * 1, 800 * 1, 1600 * 1 };
                //return new[] { 5 * H, 100 * H, 200 * H, 400 * H, 800 * H, 1600 * H };
                return new[] { 5 * K, 100 * K, 200 * K, 400 * K, 800 * K, 1600 * K };
                return new[] { 5 * K, 100 * K, 200 * K, 400 * K, 800 * K, 1600 * K, 3200 * K, 6400 * K, 12800 * K };
            }
        }

        public IEnumerable<SortedListPerformanceTestCaseConfiguration<int>> RandomIntAdd()
        {
            // Issue in NUnit: even this method is called _earlier_ than TestFixtureSetup....
            // so we can not call GetImplementations here, because FindImplementatins was not called yet :-(

            return from implementationType in ImplementationTypes
                   //from parameters in GetParameters(implementationType)
                   from size in Sizes
                   let prepare = new Action<IPerformanceTestCaseConfiguration>(c =>
                   {
                       var config = (SortedListPerformanceTestCaseConfiguration<int>) c;
                       GenerateRandomOrderUniqueInts(size, config);
                       config.Target = CreateSortedList<int>(implementationType); //, parameters);
                   })
                   let run = new Action<IPerformanceTestCaseConfiguration>(c =>
                   {
                       var config = (SortedListPerformanceTestCaseConfiguration<int>) c;
                       var target = config.Target;

                       for (var i = 0; i < size; i++)
                       {
                           target.Add(config.RandomItems[i]);
                       }
                   })
                   select new SortedListPerformanceTestCaseConfiguration<int>
                   {
                       TestName = "RandomIntAdd",
                       TargetImplementationType = implementationType,
                       Identifier = string.Format("{0}", implementationType.GetFriendlyName()), //, parameters.FirstOrDefault()),
                       Size = size,
                       Prepare = prepare,
                       Run = run,
                       IsReusable = false
                   };
        }

        public IEnumerable<SortedListPerformanceTestCaseConfiguration<int>> AscendingIntAdd()
        {
            // Issue in NUnit: even this method is called _earlier_ than TestFixtureSetup....
            // so we can not call GetImplementations here, because FindImplementatins was not called yet :-(

            return from implementationType in ImplementationTypes
                   //from parameters in GetParameters(implementationType)
                   from size in Sizes
                   let prepare = new Action<IPerformanceTestCaseConfiguration>(c =>
                   {
                       var config = (SortedListPerformanceTestCaseConfiguration<int>) c;
                       GenerateAscendingOrderUniqueInts(size, config);
                       config.Target = CreateSortedList<int>(implementationType); //, parameters);
                   })
                   let run = new Action<IPerformanceTestCaseConfiguration>(c =>
                   {
                       var config = (SortedListPerformanceTestCaseConfiguration<int>) c;
                       var target = config.Target;

                       for (var i = 0; i < size; i++)
                       {
                           target.Add(config.RandomItems[i]);
                       }
                   })
                   select new SortedListPerformanceTestCaseConfiguration<int>
                   {
                       TestName = "AscendingIntAdd",
                       TargetImplementationType = implementationType,
                       Identifier = string.Format("{0}", implementationType.GetFriendlyName()), //, parameters.FirstOrDefault()),
                       Size = size,
                       Prepare = prepare,
                       Run = run,
                       IsReusable = false
                   };
        }

        public IEnumerable<SortedListPerformanceTestCaseConfiguration<int>> DescendingIntAdd()
        {
            // Issue in NUnit: even this method is called _earlier_ than TestFixtureSetup....
            // so we can not call GetImplementations here, because FindImplementatins was not called yet :-(

            return from implementationType in ImplementationTypes
                   //from parameters in GetParameters(implementationType)
                   from size in Sizes
                   let prepare = new Action<IPerformanceTestCaseConfiguration>(c =>
                   {
                       var config = (SortedListPerformanceTestCaseConfiguration<int>) c;
                       GenerateDescendingOrderUniqueInts(size, config);
                       config.Target = CreateSortedList<int>(implementationType); //, parameters);
                   })
                   let run = new Action<IPerformanceTestCaseConfiguration>(c =>
                   {
                       var config = (SortedListPerformanceTestCaseConfiguration<int>) c;
                       var target = config.Target;

                       for (var i = 0; i < size; i++)
                       {
                           target.Add(config.RandomItems[i]);
                       }
                   })
                   select new SortedListPerformanceTestCaseConfiguration<int>
                   {
                       TestName = "DescendingIntAdd",
                       TargetImplementationType = implementationType,
                       Identifier = string.Format("{0}", implementationType.GetFriendlyName()), //, parameters.FirstOrDefault()),
                       Size = size,
                       Prepare = prepare,
                       Run = run,
                       IsReusable = false
                   };
        }

        private IEnumerable<object[]> GetParameters(Type implementationType)
        {
            if (implementationType == typeof(SortedSplitList<>) ||
                implementationType == typeof(RedBlackTreeSortedSplitList<>) ||
                implementationType == typeof(AvlTreeSortedSplitList<>))
            {
                return new[]
                {
                    new object[] { 128 },
                    new object[] { 256 },
                    new object[] { 512 },
                    new object[] { 1024 },
                    new object[] { 2048 },
                };
            }
            if (implementationType == typeof(SortedList<>))
            {
                return new[] { new object[] { false } };
            }

            return new[] { new object[] { } };
        }

        public IEnumerable<SortedListPerformanceTestCaseConfiguration<string>> RandomStringAdd()
        {
            // Issue in NUnit: even this method is called _earlier_ than TestFixtureSetup....
            // so we can not call GetImplementations here, because FindImplementatins was not called yet :-(

            return from implementationType in ImplementationTypes
                   from size in Sizes
                   let prepare = new Action<IPerformanceTestCaseConfiguration>(c =>
                   {
                       var config = (SortedListPerformanceTestCaseConfiguration<string>) c;
                       GenerateRandomPrefixEqualStrings(size, config);
                       config.Target = CreateSortedList<string>(implementationType);
                   })
                   let run = new Action<IPerformanceTestCaseConfiguration>(c =>
                   {
                       var config = (SortedListPerformanceTestCaseConfiguration<string>) c;
                       var target = config.Target;

                       for (var i = 0; i < size; i++)
                       {
                           target.Add(config.RandomItems[i]);
                       }
                   })
                   select new SortedListPerformanceTestCaseConfiguration<string>
                   {
                       TestName = "RandomStringAdd",
                       TargetImplementationType = implementationType,
                       Identifier = string.Format("{0}", implementationType.GetFriendlyName()),
                       Size = size,
                       Prepare = prepare,
                       Run = run,
                       IsReusable = false
                   };
        }

        private void GenerateRandomOrderUniqueInts(int size, SortedListPerformanceTestCaseConfiguration<int> config)
        {
            var list = new C5.ArrayList<int>(size);
            list.AddAll(Enumerable.Range(0, size));
            list.Shuffle();
            config.RandomItems = list.ToArray();
        }

        private void GenerateAscendingOrderUniqueInts(int size, SortedListPerformanceTestCaseConfiguration<int> config)
        {
            var list = new C5.ArrayList<int>(size);
            list.AddAll(Enumerable.Range(0, size));
            config.RandomItems = list.ToArray();
        }

        private void GenerateDescendingOrderUniqueInts(int size, SortedListPerformanceTestCaseConfiguration<int> config)
        {
            var list = new C5.ArrayList<int>(size);
            list.AddAll(Enumerable.Range(0, size).Reverse());
            config.RandomItems = list.ToArray();
        }

        private void GenerateRandomPrefixEqualStrings(int size, SortedListPerformanceTestCaseConfiguration<string> config)
        {
            var list = new C5.ArrayList<string>(size);
            var prefix = RandomString(100);
            list.AddAll(Enumerable.Range(0, size).Select(i => prefix + RandomString()));
            list.Shuffle();
            config.RandomItems = list.ToArray();
        }

        public static string RandomString(int length = 8)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = new Random();
            return new string(Enumerable
                .Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)])
                .ToArray()
            );
        }

        public static ISortedList<T> CreateSortedList<T>(Type implementationType, object[] parameters = null)
            where T : IComparable<T>
        {
            var genericType = implementationType.MakeGenericType(typeof(T));
            var constructorInfo = genericType.GetConstructors().First(ci => !ci.GetParameters().Any() || ci.GetParameters().All(p => p.IsOptional));
            return (ISortedList<T>) constructorInfo.Invoke(parameters ?? constructorInfo.GetParameters().Select(p => Type.Missing).ToArray());
        }
    }
}
