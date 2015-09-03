namespace SortedLists
{
    class RedBlackBinarySearchTreeTest : SortedListTestBase
    {
        protected override ISortedList<T> CreateEmptyList<T>()
        {
            return new RedBlackBinarySearchTree<T>();
        }

        protected override bool AllowsDuplicates()
        {
            return false;
        }
    }
}
