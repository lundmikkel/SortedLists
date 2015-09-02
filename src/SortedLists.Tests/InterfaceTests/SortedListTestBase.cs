namespace SortedLists.Tests.InterfaceTests
{
    using System;
    using C5;
    using Interfaces;
    using NUnit.Framework;

    // TODO: Manually check test and ensure they do as expected!

    [TestFixture]
    public abstract class SortedListTestBase
    {
        #region Fields

        protected Random Random;

        #endregion

        protected abstract ISortedList<T> CreateEmptyList<T>() where T : IComparable<T>;

        [SetUp]
        public void Setup()
        {
            var seed = (int) DateTime.Now.Ticks; // TODO
            Random = new Random(seed);
            Console.WriteLine("Random seed: {0}", seed);
        }

        #region Helper Methods

        protected ISortedList<int> CreateNonEmptyList()
        {
            var list = CreateEmptyList<int>();
            var count = Random.Next(10, 20);
            for (var i = 0; i < count; i++)
                list.Add(i);
            return list;
        }

        protected abstract bool AllowsDuplicates();

        protected int RandomInt()
        {
            return Random.Next();
        }

        protected int RandomNegativeInt()
        {
            return Random.Next(int.MinValue, 0);
        }

        protected static void AssertThrowsContractException(Action function)
        {
            try
            {
                function();
            }
            catch (Exception e)
            {
                if (e.GetType().FullName != @"System.Diagnostics.Contracts.__ContractsRuntime+ContractException")
                    throw;

                Assert.Pass();
            }

            Assert.Fail();
        }

        #endregion

        #region Test Methods

        #region Properties

        #region Count

        [Test]
        public void Count_EmptyCollection_0()
        {
            var list = CreateEmptyList<int>();
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void Count_NonEmptyCollection_Count()
        {
            var list = CreateEmptyList<int>();
            var count = Random.Next(10, 20);
            var offset = RandomInt();

            for (var i = 1; i <= count; ++i)
            {
                list.Add(i + offset);
                Assert.AreEqual(i, list.Count);
            }
        }

        [Test]
        public void Count_DuplicateItems_Count()
        {
            var list = CreateEmptyList<int>();
            var count = Random.Next(10, 20);
            var value = RandomInt();

            for (var i = 0; i < count; ++i)
                list.Add(value);

            Assert.AreEqual(list.AllowsDuplicates ? count : 1, list.Count);
        }

        #endregion

        #region IsEmpty

        [Test]
        public void IsEmpty_EmptyCollection_True()
        {
            var list = CreateEmptyList<int>();
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void IsEmpty_NonEmptyCollection_False()
        {
            var list = CreateNonEmptyList();
            Assert.False(list.IsEmpty);
        }

        #endregion

        #region AllowsDuplicates

        [Test]
        public void AllowsDuplicates_Collection_IsImplemented()
        {
            var list = CreateEmptyList<int>();
            Assert.AreEqual(AllowsDuplicates(), list.AllowsDuplicates);
        }

        #endregion

        #region IndexingSpeed

        [Test]
        public void IndexingSpeed_Collection_NotInfinite()
        {
            var list = CreateEmptyList<int>();
            Assert.AreNotEqual(Speed.PotentiallyInfinite, list.IndexingSpeed);
        }

        #endregion

        #region Indexer

        [Test]
        public void Indexer_EmptyCollection_AnyIndexThrowsContractException()
        {
            var list = CreateEmptyList<int>();

            AssertThrowsContractException(() => { var dummy = list[0]; });
            AssertThrowsContractException(() => { var dummy = list[-1]; });
            AssertThrowsContractException(() => { var dummy = list[RandomInt()]; });
        }

        [Test]
        public void Indexer_NonEmptyCollection_NegativeIndexThrowsContractException()
        {
            var list = CreateNonEmptyList();

            AssertThrowsContractException(() => { var dummy = list[-1]; });
            AssertThrowsContractException(() => { var dummy = list[RandomNegativeInt()]; });
        }

        [Test]
        public void Indexer_NonEmptyCollection_IndexGreaterOrEqualToCountThrowsContractException()
        {
            var list = CreateNonEmptyList();

            AssertThrowsContractException(() => { var dummy = list[list.Count]; });
            AssertThrowsContractException(() => { var dummy = list[Random.Next(list.Count, int.MaxValue)]; });
        }

        [Test]
        public void Indexer_NonEmptyCollection_ProperElementAtIndex()
        {
            var list = CreateEmptyList<int>();
            var count = Random.Next(10, 20);
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
                list.Add(i + offset);

            for (var i = 0; i < count; ++i)
                Assert.AreEqual(i + offset, list[i]);
        }

        #endregion

        #region First

        [Test]
        public void First_EmptyCollection_ThrowsContractException()
        {
            var list = CreateEmptyList<int>();
            AssertThrowsContractException(() => { var dummy = list.First; });
        }

        [Test]
        public void First_NonEmptyCollection_FirstElement()
        {
            var list = CreateEmptyList<int>();
            var count = Random.Next(10, 20);
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
                list.Add(i + offset);

            Assert.AreEqual(offset, list.First);
        }

        #endregion

        #region Last

        [Test]
        public void Last_EmptyCollection_ThrowsContractException()
        {
            var list = CreateEmptyList<int>();
            AssertThrowsContractException(() => { var dummy = list.Last; });
        }

        [Test]
        public void Last_NonEmptyCollection_LastElement()
        {
            var list = CreateEmptyList<int>();
            var count = Random.Next(10, 20);
            var offset = RandomInt();

            for (var i = 1; i <= count; ++i)
                list.Add(i + offset);

            Assert.AreEqual(offset + count, list.Last);
        }

        #endregion

        #endregion

        #region Enumerable

        #endregion

        #region Find

        #region IndexOf

        [Test]
        public void IndexOf_EmptyCollection_AnySearchReturnsTildeZero()
        {
            var list = CreateEmptyList<int>();
            Assert.AreEqual(~0, list.IndexOf(RandomInt()));
        }

        [Test]
        public void IndexOf_NonEmptyCollection_ProperIndexForElement()
        {
            var list = CreateEmptyList<int>();
            var count = Random.Next(10, 20);
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
                list.Add(i + offset);

            for (var i = 0; i < count; ++i)
                Assert.AreEqual(i, list.IndexOf(i + offset));
        }

        [Test]
        public void IndexOf_NonEmptyCollection_ProperTildeIndexForElement()
        {
            var list = CreateEmptyList<int>();
            var count = Random.Next(10, 20);
            var offset = RandomInt();

            for (var i = 1; i < count + 1; i += 2)
                list.Add(i + offset);

            for (var i = 0; i < count; i += 2)
                Assert.AreEqual(~(i / 2), list.IndexOf(i + offset));
        }

        [Test]
        public void IndexOf_DuplicateItems_FirstIndexForDuplicate()
        {
            var list = CreateEmptyList<int>();
            var count = Random.Next(10, 20);
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
                list.Add(i + offset);

            var value = count / 2 + offset;
            for (var i = 0; i < count; ++i)
                list.Add(value);

            Assert.AreEqual(count / 2, list.IndexOf(value));
        }

        #endregion

        #region Contains

        [Test]
        public void Contains_EmptyCollection_AnySearchReturnsFalse()
        {
            var list = CreateEmptyList<int>();
            Assert.False(list.Contains(RandomInt()));
        }

        [Test]
        public void Contains_SingleItemCollection_ContainsIt()
        {
            var list = CreateEmptyList<int>();
            var item = RandomInt();
            list.Add(item);
            Assert.That(list.Contains(item));
            var otherItem = RandomInt();
            Assert.AreEqual(item.CompareTo(otherItem) == 0, list.Contains(otherItem));
        }

        [Test]
        public void Contains_NonEmptyCollection_ContainsAll()
        {
            var list = CreateEmptyList<int>();
            var count = Random.Next(10, 20);
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
            {
                var value = i + offset;
                Assert.False(list.Contains(value));
                list.Add(value);
                Assert.That(list.Contains(value));
            }
        }

        [Test]
        public void Contains_DuplicateItems_ContainsDuplicate()
        {
            var list = CreateEmptyList<int>();
            var count = Random.Next(10, 20);
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
                list.Add(i + offset);

            var value = count / 2 + offset;
            for (var i = 1; i < count + 1; i += 2)
                list.Add(value);

            Assert.That(list.Contains(value));
        }

        [Test]
        public void Contains_AllValues_ContainsAll()
        {
            var list = CreateEmptyList<byte>();

            for (int i = byte.MinValue; i <= byte.MaxValue; ++i)
                Assert.That(list.Add((byte) i));

            for (int i = byte.MinValue; i <= byte.MaxValue; ++i)
                Assert.That(list.Contains((byte) i));
        }

        #endregion

        #endregion

        #region Extensible

        #region Add

        [Test]
        public void Add_EmptyCollection_NullThrowsContractException()
        {
            var list = CreateEmptyList<string>();
            AssertThrowsContractException(() => { var dummy = list.Add(null); });
        }

        [Test]
        public void Add_EmptyCollection_AddAllValuesSorted()
        {
            var list = CreateEmptyList<byte>();

            for (int i = byte.MinValue; i <= byte.MaxValue; ++i)
                Assert.That(list.Add((byte) i));

            for (int i = byte.MinValue; i <= byte.MaxValue; ++i)
                Assert.AreEqual(list.AllowsDuplicates, list.Add((byte) i));

            Assert.That(list.IsSorted());
        }

        [Test]
        public void Add_EmptyCollection_AddAllValuesRandomOrder()
        {
            var list = CreateEmptyList<byte>();
            var byteList = new ArrayList<byte>((int) Math.Pow(2, 16));

            for (int i = byte.MinValue; i <= byte.MaxValue; ++i)
                byteList.Add((byte) i);

            byteList.Shuffle();

            foreach (var b in byteList)
                Assert.That(list.Add(b));

            byteList.Shuffle();

            foreach (var b in byteList)
                Assert.AreEqual(list.AllowsDuplicates, list.Add(b));

            Assert.That(list.IsSorted());
        }

        [Test]
        public void Add_NonEmptyCollection_DuplicatesBasedOnComparison()
        {
            var list = CreateEmptyList<string>();
            var string1 = "Test";
            var string2 = new string(new[]{'T', 'e', 's', 't'});

            Assert.False(ReferenceEquals(string1, string2));
            Assert.That(string1.CompareTo(string2) == 0);

            Assert.That(list.Add(string1));
            Assert.AreEqual(list.AllowsDuplicates, list.Add(string2));
        }

        #endregion

        #region Clear

        [Test]
        public void Clear_EmptyCollection_Empty()
        {
            var list = CreateEmptyList<int>();
            list.Clear();
            Assert.That(list.IsEmpty);
        }

        [Test]
        public void Clear_NonEmptyCollection_Empty()
        {
            var list = CreateEmptyList<int>();
            var count = Random.Next(10, 20);
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
                list.Add(i + offset);

            Assert.False(list.IsEmpty);
            list.Clear();
            Assert.That(list.IsEmpty);
        }

        #endregion

        #endregion

        #endregion
    }
}
