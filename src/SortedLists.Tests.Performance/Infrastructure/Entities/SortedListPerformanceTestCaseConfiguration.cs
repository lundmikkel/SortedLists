namespace SortedLists
{
    using System;
    using NUnitBenchmarker.Configuration;

    public class SortedListPerformanceTestCaseConfiguration<T> : PerformanceTestCaseConfigurationBase
        where T : IComparable<T>
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
 
        public ISortedList<T> Target { get; set; }
        public T[] RandomItems { get; set; }

        #endregion
    }
}
