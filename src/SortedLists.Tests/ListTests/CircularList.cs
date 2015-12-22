namespace SortedLists.Tests.ListTests
{
    using Lists;
    using NUnit.Framework;

    [TestFixture]
    class CircularList
    {
        [Test]
        public void test()
        {
            var list = new CircularList<string>();

            list.Add("A");
            list.Add("B");
            list.Add("C");
            list.Add("D");
            list.Add("E");
            list.Add("F");
            list.Add("G");
            list.Add("H");

            list.moveHead(2, 3);
        }

    }
}
