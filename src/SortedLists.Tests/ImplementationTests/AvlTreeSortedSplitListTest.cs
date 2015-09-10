namespace SortedLists
{
    public class AvlTreeSortedSplitListTest : SortedListTestBase
    {
        protected override ISortedList<T> CreateEmptyList<T>()
        {
            return new AvlTreeSortedSplitList<T>(8);
        }

        protected override bool AllowsDuplicates()
        {
            return false;
        }
    }
}
