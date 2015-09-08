namespace SortedLists
{
    class DoublyLinkedRedBlackBinarySearchTreeTest : SortedListTestBase
    {
        protected override ISortedList<T> CreateEmptyList<T>()
        {
            return new DoublyLinkedRedBlackBinarySearchTree<T>();
        }

        protected override bool AllowsDuplicates()
        {
            return false;
        }
    }
}
