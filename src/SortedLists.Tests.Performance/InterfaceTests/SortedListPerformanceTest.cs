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

        [Test, TestCaseSource(typeof(SortedListPerformanceTestFactory), "RandomIntAdd")]
        public void RandomIntAdd(SortedListPerformanceTestCaseConfiguration<int> config)
        {
            config.Benchmark(config.TestName, config.Size, 3);
        }

        [Test, TestCaseSource(typeof(SortedListPerformanceTestFactory), "RandomStringAdd")]
        public void RandomStringAdd(SortedListPerformanceTestCaseConfiguration<string> config)
        {
            config.Benchmark(config.TestName, config.Size, 3);
        }
    }
}
