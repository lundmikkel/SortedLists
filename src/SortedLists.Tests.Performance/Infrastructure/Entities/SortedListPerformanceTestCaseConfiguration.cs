namespace SortedLists
{
    using NUnitBenchmarker.Configuration;

    public class SortedListPerformanceTestCaseConfiguration : PerformanceTestCaseConfigurationBase
    {
        #region Methods
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return TestName;
        }
        #endregion

        #region Properties
 
        public ISortedList<int> Target { get; set; }
        public int[] RandomIntegers { get; set; }
        #endregion
    }
}
