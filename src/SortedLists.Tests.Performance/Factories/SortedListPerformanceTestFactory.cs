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
                    typeof (SortedList<>),
                    typeof (RedBlackBinarySearchTree<>),
                    typeof (SortedSplitList<>),
                };
            }
        }

        private static IEnumerable<int> Sizes
        {
            get
            {
                //return new[] { 5 * 1, 100 * 1, 200 * 1, 400 * 1, 800 * 1, 1600 * 1 };
                return new[] { 5 * H, 100 * H, 200 * H, 400 * H, 800 * H, 1600 * H };
                return new[] { 5 * K, 100 * K, 200 * K, 400 * K, 800 * K, 1600 * K };
            }
        }

        public IEnumerable<SortedListPerformanceTestCaseConfiguration<int>> RandomIntAdd()
        {
            // Issue in NUnit: even this method is called _earlier_ than TestFixtureSetup....
            // so we can not call GetImplementations here, because FindImplementatins was not called yet :-(

            return from implementationType in ImplementationTypes
                   from size in Sizes
                   let prepare = new Action<IPerformanceTestCaseConfiguration>(c =>
                   {
                       var config = (SortedListPerformanceTestCaseConfiguration<int>) c;
                       GenerateRandomOrderUniqueInts(size, config);
                       config.Target = CreateSortedList<int>(implementationType);
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

        public static ISortedList<T> CreateSortedList<T>(Type implementationType, object[] parameters = null)
            where T : IComparable<T>
        {
            var genericType = implementationType.MakeGenericType(typeof(T));
            var constructorInfo = genericType.GetConstructors().First(ci => !ci.GetParameters().Any() || ci.GetParameters().All(p => p.IsOptional));
            return (ISortedList<T>) constructorInfo.Invoke(parameters ?? constructorInfo.GetParameters().Select(p => Type.Missing).ToArray());
        }
    }
}
