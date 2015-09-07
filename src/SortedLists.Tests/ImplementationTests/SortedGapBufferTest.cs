namespace SortedLists
{
    abstract class AbstractSortedGapBufferTest : SortedListTestBase
    {
        protected override ISortedList<T> CreateEmptyList<T>()
        {
            return new SortedGapBuffer<T>(AllowsDuplicates());
        }
    }

    class SortedGapBufferListTest : AbstractSortedGapBufferTest
    {
        protected override bool AllowsDuplicates()
        {
            return true;
        }
    }

    class SortedGapBufferSetTest : AbstractSortedGapBufferTest
    {
        protected override bool AllowsDuplicates()
        {
            return false;
        }
    }
}
