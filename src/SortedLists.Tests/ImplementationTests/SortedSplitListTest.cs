namespace SortedLists
{
    public class SortedSplitListTest : SortedListTestBase
    {
        protected override ISortedList<T> CreateEmptyList<T>()
        {
            return new SortedSplitList<T>(8);
        }

        protected override bool AllowsDuplicates()
        {
            return false;
        }
    }
}
