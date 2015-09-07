namespace SortedLists
{
    using NUnit.Framework;
    using NUnitBenchmarker;
    
    [TestFixture]
    public class SortedListPerformanceTest
    {
        [TestFixtureSetUp]
        public void TestFixture()
        {
            Benchmarker.Init();
        }

        [Test, TestCaseSource(typeof(SortedListPerformanceTestFactory), "TestCases")]
        public void Add(SortedListPerformanceTestCaseConfiguration config)
        {
            config.Benchmark(config.TestName, config.Size, 1);
        }
    }
}
