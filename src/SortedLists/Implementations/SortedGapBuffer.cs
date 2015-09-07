namespace SortedLists
{
    using Slusser.Collections.Generic;

    public class SortedGapBuffer<T> : SortedList<T>
        where T : System.IComparable<T>
    {
        public SortedGapBuffer(bool allowsDuplicates = false) : base(allowsDuplicates) { }

        protected override System.Collections.Generic.IList<T> CreateEmptyList()
        {
            return new GapBuffer<T>();
        }
    }
}
