namespace SortedLists
{
    abstract class AbstractSortedArrayListTest : SortedListTestBase
    {
        protected override ISortedList<T> CreateEmptyList<T>()
        {
            return new SortedArrayList<T>(AllowsDuplicates());
        }
    }

    class SortedArrayListTest : AbstractSortedArrayListTest
    {
        protected override bool AllowsDuplicates()
        {
            return true;
        }
    }

    class SortedArraySetTest : AbstractSortedArrayListTest
    {
        protected override bool AllowsDuplicates()
        {
            return false;
        }
    }
}
