namespace SortedLists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Catel.Collections;
    using NUnitBenchmarker;
    using NUnitBenchmarker.Configuration;

    public class SortedListPerformanceTestFactory
    {
        private const int K = 1000;

        public Type[] ImplementationTypes
        {
            get
            {
                return new[]{
                    //typeof (SortedList<int>),
                    typeof (RedBlackBinarySearchTree<int>),
                    typeof (SortedSplitList<int>),
                };
            }
        }

        private static IEnumerable<int> Sizes
        {
            get
            {
                return new[] { 5 * 1, 100 * 1, 200 * 1, 400 * 1, 800 * 1, 1600 * 1 };
                return new[] { 5 * K, 100 * K, 200 * K, 400 * K, 800 * K, 1600 * K };
            }
        }

        public IEnumerable<SortedListPerformanceTestCaseConfiguration> TestCases()
        {
            // Issue in NUnit: even this method is called _earlier_ than TestFixtureSetup....
            // so we can not call GetImplementations here, because FindImplementatins was not called yet :-(

            return from implementationType in ImplementationTypes
                   from size in Sizes
                   let prepare = new Action<IPerformanceTestCaseConfiguration>(c =>
                   {
                       var config = (SortedListPerformanceTestCaseConfiguration) c;
                       PrepareAdd(size, implementationType, config);

                       // JIT warm-up
                       var list = CreateSortedList<int>(config.Target.GetType());
                       Enumerable.Range(0, 100).ForEach(i => list.Add(i));
                   })
                   let run = new Action<IPerformanceTestCaseConfiguration>(c =>
                   {
                       var config = (SortedListPerformanceTestCaseConfiguration) c;
                       var target = config.Target;

                       for (var i = 0; i < size; i++)
                       {
                           target.Add(config.RandomIntegers[i]);
                       }
                   })
                   select new SortedListPerformanceTestCaseConfiguration
                   {
                       TestName = "Add",
                       TargetImplementationType = implementationType,
                       Identifier = string.Format("{0}", implementationType.GetFriendlyName()),
                       Size = size,
                       Prepare = prepare,
                       Run = run,
                       IsReusable = false
                   };
        }

        private void PrepareAdd(int size, Type type, SortedListPerformanceTestCaseConfiguration config)
        {
            var list = new C5.ArrayList<int>(size);
            list.AddAll(Enumerable.Range(0, size));
            list.Shuffle();

            config.RandomIntegers = list.ToArray();

            config.Target = CreateSortedList<int>(type);
        }

        public static ISortedList<T> CreateSortedList<T>(Type implementationType)
            where T : IComparable<T>
        {
            var constructorInfo = implementationType.GetConstructors().First(ci => !ci.GetParameters().Any() || ci.GetParameters().All(p => p.IsOptional));
            var parameters = constructorInfo.GetParameters().Select(p => Type.Missing).ToArray();
            return (ISortedList<T>) constructorInfo.Invoke(parameters);
        }
    }
}
