namespace SortedLists
{
    class RedBlackTreeTest : SortedListTestBase
    {
        protected override ISortedList<T> CreateEmptyList<T>()
        {
            return new RedBlackTree<T>();
        }

        protected override bool AllowsDuplicates()
        {
            return false;
        }
    }
}
