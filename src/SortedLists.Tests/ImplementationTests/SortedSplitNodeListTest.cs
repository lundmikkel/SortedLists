namespace SortedLists
{
    public class SortedSplitNodeListTest : SortedListTestBase
    {
        protected override ISortedList<T> CreateEmptyList<T>()
        {
            return new SortedSplitNodeList<T>(8);
        }

        protected override bool AllowsDuplicates()
        {
            return false;
        }
    }
}
