namespace SortedLists
{
    class DoublyLinkedRedBlackTreeTest : SortedListTestBase
    {
        protected override ISortedList<T> CreateEmptyList<T>()
        {
            return new DoublyLinkedRedBlackTree<T>();
        }

        protected override bool AllowsDuplicates()
        {
            return false;
        }
    }
}
