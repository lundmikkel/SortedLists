namespace SortedLists
{
    public class SortedArrayList<T> : SortedList<T>
        where T : System.IComparable<T>
    {
        public SortedArrayList(bool allowsDuplicates = false) : base(allowsDuplicates) { }

        protected override System.Collections.Generic.IList<T> CreateEmptyList()
        {
            return new C5.ArrayList<T>();
        }
    }
}
