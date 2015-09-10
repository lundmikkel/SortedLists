namespace SortedLists
{
    public class TreeSortedSplitListTest : SortedListTestBase
    {
        protected override ISortedList<T> CreateEmptyList<T>()
        {
            return new TreeSortedSplitList<T>(8);
        }

        protected override bool AllowsDuplicates()
        {
            return false;
        }
    }
}
