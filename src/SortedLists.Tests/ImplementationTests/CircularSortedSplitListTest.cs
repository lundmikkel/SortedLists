namespace SortedLists
{
    public class CircularSortedSplitListTest : SortedListTestBase
    {
        protected override ISortedList<T> CreateEmptyList<T>()
        {
            return new CircularSortedSplitList<T>(8);
        }

        protected override bool AllowsDuplicates()
        {
            return false;
        }
    }
}
