namespace SortedLists
{
    class DoublyLinkedAvlTreeTest : SortedListTestBase
    {
        protected override ISortedList<T> CreateEmptyList<T>()
        {
            return new DoublyLinkedAvlTree<T>();
        }

        protected override bool AllowsDuplicates()
        {
            return false;
        }
    }
}
