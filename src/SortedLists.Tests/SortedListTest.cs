namespace SortedLists.Tests
{
    using Interfaces;
    using InterfaceTests;

    abstract class AbstractSortedListTest : SortedListTestBase
    {
        protected override ISortedList<T> CreateEmptyList<T>()
        {
            return new SortedList<T>(AllowsDuplicates());
        }
    }

    class SortedListTest : AbstractSortedListTest {
        protected override bool AllowsDuplicates()
        {
            return true;
        }
    }

    class SortedSetTest : AbstractSortedListTest {
        protected override bool AllowsDuplicates()
        {
            return false;
        }
    }
}
