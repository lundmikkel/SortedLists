namespace SortedLists
{
    public class RedBlackTreeSortedSplitListTest : SortedListTestBase
    {
        protected override ISortedList<T> CreateEmptyList<T>()
        {
            return new RedBlackTreeSortedSplitList<T>(8);
        }

        protected override bool AllowsDuplicates()
        {
            return false;
        }
    }
}
