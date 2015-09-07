namespace SortedLists
{
    class SortedArrayTest : SortedListTestBase
    {
        protected override ISortedList<T> CreateEmptyList<T>()
        {
            return new SortedArray<T>();
        }

        protected override bool AllowsDuplicates()
        {
            return false;
        }
    }
}
