namespace SortedLists
{
    public class SortedSplitGapBufferTest : SortedListTestBase
    {
        protected override ISortedList<T> CreateEmptyList<T>()
        {
            return new SortedSplitGapBuffer<T>(8);
        }

        protected override bool AllowsDuplicates()
        {
            return false;
        }
    }
}
