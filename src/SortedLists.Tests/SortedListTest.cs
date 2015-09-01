namespace SortedLists.Tests
{
    using Interfaces;
    using InterfaceTests;

    abstract class AbstractSortedListTest : SortedListTestBase
    {
        protected override ISortedList<int> CreateEmptyList()
        {
            return new SortedList<int>(AllowsDuplicates());
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
