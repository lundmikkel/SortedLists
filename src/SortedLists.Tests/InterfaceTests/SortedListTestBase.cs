namespace SortedLists.Tests.InterfaceTests
{
    using System;
    using System.Linq;
    using C5;
    using Interfaces;
    using NUnit.Framework;

    // TODO: Manually check test and ensure tests do as expected!
    // TODO: Make class that fails all tests!

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
            seed = -1958645440;
            Random = new Random(seed);
            Console.WriteLine("Random seed: {0}", seed);
        }

        #region Helper Methods

        protected ISortedList<int> CreateNonEmptyList()
        {
            var list = CreateEmptyList<int>();
            var count = RandomCount();
            var offset = RandomInt();
            for (int i = 0, j = 0; i < count; ++i, j += Random.Next(1, 10))
                list.Add(j + offset);
            return list;
        }

        protected abstract bool AllowsDuplicates();

        protected int RandomInt()
        {
            return Random.Next(int.MinValue, int.MaxValue);
        }

        protected int RandomCount()
        {
            return Random.Next(10, 20);
        }

        protected int RandomNegativeInt()
        {
            return Random.Next(int.MinValue, 0);
        }

        protected static void AssertThrowsContractException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (e.GetType().FullName.Equals(@"System.Diagnostics.Contracts.__ContractsRuntime+ContractException")
                    && e.Message.StartsWith("Precondition failed:"))
                    return;

                throw;
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
            var count = RandomCount();
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
            var count = RandomCount();
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
            Assert.That(CreateEmptyList<int>().IsEmpty);
            Assert.That(CreateEmptyList<string>().IsEmpty);
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
        public void AllowsDuplicates_EmptyCollection_IsImplemented()
        {
            var list = CreateEmptyList<int>();
            Assert.AreEqual(AllowsDuplicates(), list.AllowsDuplicates);
        }

        [Test]
        public void AllowsDuplicates_NonEmptyCollection_IsImplemented()
        {
            var list = CreateNonEmptyList();
            Assert.AreEqual(AllowsDuplicates(), list.AllowsDuplicates);
        }

        #endregion

        #region IndexingSpeed

        [Test]
        public void IndexingSpeed_EmptyCollection_NotInfinite()
        {
            var list = CreateEmptyList<int>();
            Assert.AreNotEqual(Speed.PotentiallyInfinite, list.IndexingSpeed);
        }

        [Test]
        public void IndexingSpeed_NonEmptyCollection_NotInfinite()
        {
            var list = CreateNonEmptyList();
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
            AssertThrowsContractException(() => { var dummy = list[int.MinValue]; });
            AssertThrowsContractException(() => { var dummy = list[int.MaxValue]; });
        }

        [Test]
        public void Indexer_NonEmptyCollection_NegativeIndexThrowsContractException()
        {
            var list = CreateNonEmptyList();

            AssertThrowsContractException(() => { var dummy = list[-1]; });
            AssertThrowsContractException(() => { var dummy = list[RandomNegativeInt()]; });
            AssertThrowsContractException(() => { var dummy = list[int.MinValue]; });
        }

        [Test]
        public void Indexer_NonEmptyCollection_IndexGreaterOrEqualToCountThrowsContractException()
        {
            var list = CreateNonEmptyList();

            AssertThrowsContractException(() => { var dummy = list[list.Count]; });
            AssertThrowsContractException(() => { var dummy = list[Random.Next(list.Count, int.MaxValue)]; });
            AssertThrowsContractException(() => { var dummy = list[int.MaxValue]; });
        }

        [Test]
        public void Indexer_NonEmptyCollection_ProperElementAtIndex()
        {
            var list = CreateEmptyList<int>();
            var count = RandomCount();
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
        public void First_SingleObjectCollection_FirstAndLastEqual()
        {
            var list = CreateEmptyList<int>();
            list.Add(RandomInt());

            Assert.AreEqual(list.First, list.Last);
        }

        [Test]
        public void First_NonEmptyCollection_FirstElement()
        {
            var list = CreateEmptyList<int>();
            var count = RandomCount();
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
                list.Add(i + offset);

            Assert.AreEqual(offset, list.First);
        }

        [Test]
        public void First_NonEmptyCollection_LinqFirstElement()
        {
            var list = CreateEmptyList<int>();
            var count = RandomCount();

            for (var i = 0; i < count; ++i)
                list.Add(RandomInt());

            Assert.AreEqual(list.First(), list.First);
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
            var count = RandomCount();
            var offset = RandomInt();

            for (var i = 1; i <= count; ++i)
                list.Add(i + offset);

            Assert.AreEqual(offset + count, list.Last);
        }

        [Test]
        public void Last_NonEmptyCollection_LinqLastElement()
        {
            var list = CreateEmptyList<int>();
            var count = RandomCount();

            for (var i = 1; i <= count; ++i)
                list.Add(RandomInt());

            Assert.AreEqual(list.Last(), list.Last);
        }

        #endregion

        #endregion

        #region Enumerable

        #region EnumerateFromIndex

        [Test]
        public void EnumerateFromIndex_EmptyCollection_AnyIndexThrowsContractException()
        {
            var list = CreateEmptyList<int>();

            AssertThrowsContractException(() => { var dummy = list.EnumerateFromIndex(0); });
            AssertThrowsContractException(() => { var dummy = list.EnumerateFromIndex(-1); });
            AssertThrowsContractException(() => { var dummy = list.EnumerateFromIndex(RandomInt()); });
            AssertThrowsContractException(() => { var dummy = list.EnumerateFromIndex(int.MinValue); });
            AssertThrowsContractException(() => { var dummy = list.EnumerateFromIndex(int.MaxValue); });
        }

        [Test]
        public void EnumerateFromIndex_NonEmptyCollection_NegativeIndexThrowsContractException()
        {
            var list = CreateNonEmptyList();

            AssertThrowsContractException(() => { list.EnumerateFromIndex(-1); });
            AssertThrowsContractException(() => { list.EnumerateFromIndex(RandomNegativeInt()); });
            AssertThrowsContractException(() => { list.EnumerateFromIndex(int.MinValue); });
        }

        [Test]
        public void EnumerateFromIndex_NonEmptyCollection_IndexGreaterOrEqualToCountThrowsContractException()
        {
            var list = CreateNonEmptyList();

            AssertThrowsContractException(() => { list.EnumerateFromIndex(list.Count); });
            AssertThrowsContractException(() => { list.EnumerateFromIndex(Random.Next(list.Count, int.MaxValue)); });
            AssertThrowsContractException(() => { list.EnumerateFromIndex(int.MaxValue); });
        }

        [Test]
        public void EnumerateFromIndex_NonEmptyCollection_ProperEnumerableAtIndex()
        {
            var list = CreateEmptyList<int>();
            var expected = new ArrayList<int>();
            var count = RandomCount();
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
            {
                var value = i + offset;
                list.Add(value);
                expected.Add(value);
            }

            for (var i = 0; i < count; ++i)
                CollectionAssert.AreEqual(expected.Skip(i), list.EnumerateFromIndex(i));
        }

        [Test]
        public void EnumerateFromIndex_NonEmptyCollection_RandomInsertionSortedCollection()
        {
            var list = CreateEmptyList<int>();
            var expected = new ArrayList<int>();
            var count = RandomCount();
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
                expected.Add(i + offset);

            expected.Shuffle();

            foreach (var i in expected)
                list.Add(i);

            expected.Sort();

            for (var i = 0; i < count; ++i)
                CollectionAssert.AreEqual(expected.Skip(i), list.EnumerateFromIndex(i));
        }

        [Test]
        public void EnumerateFromIndex_DuplicateItems_RandomInsertionSortedCollection()
        {
            var list = CreateEmptyList<int>();
            var expected = new ArrayList<int>();
            var count = RandomCount();
            var offset = RandomInt();
            var duplicate = count / 2 + offset;

            for (var i = 0; i < count; ++i)
            {
                expected.Add(i + offset);

                if (AllowsDuplicates())
                    expected.Add(duplicate);
            }

            expected.Shuffle();

            foreach (var i in expected)
                list.Add(i);

            expected.Sort();

            for (var i = 0; i < count; ++i)
                CollectionAssert.AreEqual(expected.Skip(i), list.EnumerateFromIndex(i));
        }

        #endregion

        #region EnumerateRange

        [Test]
        public void EnumerateRange_EmptyCollection_AnyIndexThrowsContractException()
        {
            var list = CreateEmptyList<int>();

            AssertThrowsContractException(() => { list.EnumerateRange(0, 1); });
            AssertThrowsContractException(() => { list.EnumerateRange(-1, 0); });
            AssertThrowsContractException(() => { list.EnumerateRange(RandomInt(), RandomInt()); });
            AssertThrowsContractException(() => { list.EnumerateRange(int.MinValue, int.MaxValue); });
        }

        [Test]
        public void EnumerateRange_NonEmptyCollection_NegativeIndexThrowsContractException()
        {
            var list = CreateNonEmptyList();

            AssertThrowsContractException(() => { list.EnumerateRange(-1, 1); });
            AssertThrowsContractException(() => { list.EnumerateRange(RandomNegativeInt(), 1); });
            AssertThrowsContractException(() => { list.EnumerateRange(int.MinValue, 1); });
        }

        [Test]
        public void EnumerateRange_NonEmptyCollection_EqualIndicesThrowsContractException()
        {
            var list = CreateNonEmptyList();

            AssertThrowsContractException(() => { list.EnumerateRange(0, 0); });
            AssertThrowsContractException(() => { list.EnumerateRange(list.Count, list.Count); });
        }

        [Test]
        public void EnumerateRange_NonEmptyCollection_IndexGreaterOrEqualToCountThrowsContractException()
        {
            var list = CreateNonEmptyList();

            AssertThrowsContractException(() => { list.EnumerateRange(0, list.Count + 1); });
            AssertThrowsContractException(() => { list.EnumerateRange(0, Random.Next(list.Count + 1, int.MaxValue)); });
            AssertThrowsContractException(() => { list.EnumerateRange(0, int.MaxValue); });
        }

        [Test]
        public void EnumerateRange_NonEmptyCollection_ProperEnumerableAtIndex()
        {
            var list = CreateEmptyList<int>();
            var expected = new ArrayList<int>();
            var count = RandomCount();
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
            {
                var value = i + offset;
                list.Add(value);
                expected.Add(value);
            }

            for (var i = 0; i < count - 1; ++i)
                for (var j = i + 1; j < count; ++j)
                    CollectionAssert.AreEqual(expected.Skip(i).Take(j - i), list.EnumerateRange(i, j));
        }

        [Test]
        public void EnumerateRange_NonEmptyCollection_RandomInsertionSortedCollection()
        {
            var list = CreateEmptyList<int>();
            var expected = new ArrayList<int>();
            var count = RandomCount();
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
                expected.Add(i + offset);

            expected.Shuffle();

            foreach (var i in expected)
                list.Add(i);

            expected.Sort();

            for (var i = 0; i < count - 1; ++i)
                for (var j = i + 1; j < count; ++j)
                    CollectionAssert.AreEqual(expected.Skip(i).Take(j - i), list.EnumerateRange(i, j));
        }

        [Test]
        public void EnumerateRange_DuplicateItems_RandomInsertionSortedCollection()
        {
            var list = CreateEmptyList<int>();
            var expected = new ArrayList<int>();
            var count = RandomCount();
            var offset = RandomInt();
            var duplicate = count / 2 + offset;

            for (var i = 0; i < count; ++i)
            {
                expected.Add(i + offset);

                if (AllowsDuplicates())
                    expected.Add(duplicate);
            }

            expected.Shuffle();

            foreach (var i in expected)
                list.Add(i);

            expected.Sort();

            for (var i = 0; i < count - 1; ++i)
                for (var j = i + 1; j < count; ++j)
                    CollectionAssert.AreEqual(expected.Skip(i).Take(j - i), list.EnumerateRange(i, j));
        }

        #endregion

        #region EnumerateBackwardsFromIndex

        [Test]
        public void EnumerateBackwardsFromIndex_EmptyCollection_AnyIndexThrowsContractException()
        {
            var list = CreateEmptyList<int>();

            AssertThrowsContractException(() => { list.EnumerateBackwardsFromIndex(0); });
            AssertThrowsContractException(() => { list.EnumerateBackwardsFromIndex(-1); });
            AssertThrowsContractException(() => { list.EnumerateBackwardsFromIndex(RandomInt()); });
            AssertThrowsContractException(() => { list.EnumerateBackwardsFromIndex(int.MinValue); });
            AssertThrowsContractException(() => { list.EnumerateBackwardsFromIndex(int.MaxValue); });
        }

        [Test]
        public void EnumerateBackwardsFromIndex_NonEmptyCollection_NegativeIndexThrowsContractException()
        {
            var list = CreateNonEmptyList();

            AssertThrowsContractException(() => { list.EnumerateBackwardsFromIndex(-1); });
            AssertThrowsContractException(() => { list.EnumerateBackwardsFromIndex(RandomNegativeInt()); });
            AssertThrowsContractException(() => { list.EnumerateBackwardsFromIndex(int.MinValue); });
        }

        [Test]
        public void EnumerateBackwardsFromIndex_NonEmptyCollection_IndexGreaterOrEqualToCountThrowsContractException()
        {
            var list = CreateNonEmptyList();

            AssertThrowsContractException(() => { list.EnumerateBackwardsFromIndex(list.Count); });
            AssertThrowsContractException(() => { list.EnumerateBackwardsFromIndex(Random.Next(list.Count, int.MaxValue)); });
            AssertThrowsContractException(() => { list.EnumerateBackwardsFromIndex(int.MaxValue); });
        }

        [Test]
        public void EnumerateBackwardsFromIndex_NonEmptyCollection_ProperEnumerableAtIndex()
        {
            var list = CreateEmptyList<int>();
            var expected = new ArrayList<int>();
            var count = RandomCount();
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
            {
                var value = i + offset;
                list.Add(value);
                expected.Add(value);
            }

            for (var i = 0; i < count; ++i)
                CollectionAssert.AreEqual(expected.Take(i + 1).Reverse(), list.EnumerateBackwardsFromIndex(i));
        }

        [Test]
        public void EnumerateBackwardsFromIndex_NonEmptyCollection_RandomInsertionSortedCollection()
        {
            var list = CreateEmptyList<int>();
            var expected = new ArrayList<int>();
            var count = RandomCount();
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
                expected.Add(i + offset);

            expected.Shuffle();

            foreach (var i in expected)
                list.Add(i);

            expected.Sort();

            for (var i = 0; i < count; ++i)
                CollectionAssert.AreEqual(expected.Take(i + 1).Reverse(), list.EnumerateBackwardsFromIndex(i));
        }

        [Test]
        public void EnumerateBackwardsFromIndex_DuplicateItems_RandomInsertionSortedCollection()
        {
            var list = CreateEmptyList<int>();
            var expected = new ArrayList<int>();
            var count = RandomCount();
            var offset = RandomInt();
            var duplicate = count / 2 + offset;

            for (var i = 0; i < count; ++i)
            {
                expected.Add(i + offset);

                if (AllowsDuplicates())
                    expected.Add(duplicate);
            }

            expected.Shuffle();

            foreach (var i in expected)
                list.Add(i);

            expected.Sort();

            for (var i = 0; i < count; ++i)
                CollectionAssert.AreEqual(expected.Take(i + 1).Reverse(), list.EnumerateBackwardsFromIndex(i));
        }

        #endregion

        #region GetEnumerator

        [Test]
        public void GetEnumerator_EmptyCollection_IsEmpty()
        {
            CollectionAssert.IsEmpty(CreateEmptyList<int>());
            CollectionAssert.IsEmpty(CreateEmptyList<string>());
        }

        [Test]
        public void GetEnumerator_NonEmptyCollection_OrderedInsertionSortedCollection()
        {
            var list = CreateEmptyList<int>();
            var expected = new ArrayList<int>();
            var count = RandomCount();
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
            {
                var value = i + offset;
                list.Add(value);
                expected.Add(value);
            }

            CollectionAssert.AreEqual(expected, list);
        }

        [Test]
        public void GetEnumerator_NonEmptyCollection_RandomInsertionSortedCollection()
        {
            var list = CreateEmptyList<int>();
            var expected = new ArrayList<int>();
            var count = RandomCount();
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
                expected.Add(i + offset);

            expected.Shuffle();

            foreach (var i in expected)
                list.Add(i);

            expected.Sort();

            CollectionAssert.AreEqual(expected, list);
        }

        [Test]
        public void GetEnumerator_NonEmptyCollection_MatchesIndexer()
        {
            var list = CreateEmptyList<int>();
            var expected = new ArrayList<int>();
            var count = RandomCount();
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
                expected.Add(i + offset);

            expected.Shuffle();

            foreach (var i in expected)
                list.Add(i);

            expected.Sort();

            var index = 0;
            foreach (var item in list)
                Assert.AreEqual(expected[index++], item);
        }

        #endregion

        #region EnumerateBackwards

        [Test]
        public void EnumerateBackwards_EmptyCollection_IsEmpty()
        {
            CollectionAssert.IsEmpty(CreateEmptyList<int>().EnumerateBackwards());
            CollectionAssert.IsEmpty(CreateEmptyList<string>().EnumerateBackwards());
        }

        [Test]
        public void EnumerateBackwards_NonEmptyCollection_OrderedInsertionSortedCollection()
        {
            var list = CreateEmptyList<int>();
            var expected = new ArrayList<int>();
            var count = RandomCount();
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
            {
                var value = i + offset;
                list.Add(value);
                expected.Add(value);
            }

            CollectionAssert.AreEqual(Enumerable.Reverse(expected), list.EnumerateBackwards());
        }

        [Test]
        public void EnumerateBackwards_NonEmptyCollection_RandomInsertionSortedCollection()
        {
            var list = CreateEmptyList<int>();
            var expected = new ArrayList<int>();
            var count = RandomCount();
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
                expected.Add(i + offset);

            expected.Shuffle();

            foreach (var i in expected)
                list.Add(i);

            expected.Sort();
            expected.Reverse();

            CollectionAssert.AreEqual(expected, list.EnumerateBackwards());
        }

        [Test]
        public void EnumerateBackwards_NonEmptyCollection_MatchesIndexer()
        {
            var list = CreateEmptyList<int>();
            var expected = new ArrayList<int>();
            var count = RandomCount();
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
                expected.Add(i + offset);

            expected.Shuffle();

            foreach (var i in expected)
                list.Add(i);

            expected.Sort();

            var index = count;
            foreach (var item in list.EnumerateBackwards())
                Assert.AreEqual(expected[--index], item);
        }

        #endregion

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
            var count = RandomCount();
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
            var count = RandomCount();
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
                list.Add(1 + 2 * i + offset);

            for (var i = 0; i < count; ++i)
                Assert.AreEqual(~i, list.IndexOf(2 * i + offset));
        }

        [Test]
        public void IndexOf_DuplicateItems_FirstIndexForDuplicate()
        {
            var list = CreateEmptyList<int>();
            var count = RandomCount();
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
                list.Add(i + offset);

            var value = count / 2 + offset;
            for (var i = 0; i < count; ++i)
                list.Add(value);

            Assert.AreEqual(count / 2, list.IndexOf(value));
        }

        [Test]
        public void IndexOf_AllValues_IndexMatchesValue()
        {
            var list = CreateEmptyList<byte>();

            for (int i = byte.MinValue; i <= byte.MaxValue; ++i)
                Assert.That(list.Add((byte) i));

            for (int i = byte.MinValue; i <= byte.MaxValue; ++i)
                Assert.AreEqual(i, list.IndexOf((byte) i));
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
            var expected = new ArrayList<int>();
            var count = RandomCount();
            var offset = RandomInt();

            for (var i = 0; i < count; ++i)
                expected.Add(i + offset);

            expected.Shuffle();

            foreach (var i in expected)
            {
                Assert.False(list.Contains(i));
                list.Add(i);
                Assert.That(list.Contains(i));
            }
        }

        [Test]
        public void Contains_DuplicateItems_ContainsDuplicate()
        {
            var list = CreateEmptyList<int>();
            var count = RandomCount();
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
            var count = 1 << 8;
            var byteList = new ArrayList<byte>(count);

            for (int i = byte.MinValue; i <= byte.MaxValue; ++i)
                byteList.Add((byte) i);

            byteList.Shuffle();

            foreach (var b in byteList)
                Assert.That(list.Add(b));

            Assert.AreEqual(count, list.Count);

            byteList.Shuffle();

            foreach (var b in byteList)
                Assert.AreEqual(list.AllowsDuplicates, list.Add(b));

            Assert.AreEqual(list.AllowsDuplicates ? count * 2 : count, list.Count);

            Assert.That(list.IsSorted());
        }

        [Test]
        public void Add_NonEmptyCollection_DuplicatesBasedOnComparison()
        {
            var list = CreateEmptyList<string>();
            var string1 = "Test";
            var string2 = new string(new[] { 'T', 'e', 's', 't' });

            Assert.False(ReferenceEquals(string1, string2));
            Assert.That(string1.CompareTo(string2) == 0);

            Assert.That(list.Add(string1));
            Assert.AreEqual(list.AllowsDuplicates, list.Add(string2));
        }

        #endregion

        #region Remove

        [Test]
        public void Remove_EmptyCollection_NullThrowsContractException()
        {
            var list = CreateEmptyList<string>();
            AssertThrowsContractException(() => { var dummy = list.Remove(null); });
        }

        [Test]
        public void Remove_EmptyCollection_FalseForAnyRemove()
        {
            var list = CreateEmptyList<int>();
            Assert.False(list.Remove(RandomInt()));
        }

        [Test]
        public void Remove_NonEmptyCollection_RemoveAllValuesSorted()
        {
            var list = CreateEmptyList<byte>();
            var count = 1 << 8;

            for (int i = byte.MinValue; i <= byte.MaxValue; ++i)
                Assert.That(list.Add((byte) i));

            Assert.AreEqual(count, list.Count);

            for (int i = byte.MinValue; i <= byte.MaxValue; ++i)
                Assert.That(list.Remove((byte) i));

            Assert.That(list.IsEmpty);

            for (int i = byte.MinValue; i <= byte.MaxValue; ++i)
                Assert.False(list.Remove((byte) i));
        }

        [Test]
        public void Remove_EmptyCollection_RemoveAllValuesRandomOrder()
        {
            var list = CreateEmptyList<byte>();
            var count = 1 << 8;
            var byteList = new ArrayList<byte>(count);

            for (int i = byte.MinValue; i <= byte.MaxValue; ++i)
            {
                var b = (byte) i;
                byteList.Add(b);
                list.Add(b);
            }

            byteList.Shuffle();

            foreach (var b in byteList)
                Assert.That(list.Remove(b));

            Assert.That(list.IsEmpty);

            byteList.Shuffle();

            foreach (var b in byteList)
                Assert.False(list.Remove(b));
        }

        [Test]
        public void Remove_NonEmptyCollection_DuplicatesBasedOnComparison()
        {
            var list = CreateEmptyList<string>();
            var string1 = "Test";
            var string2 = new string(new[] { 'T', 'e', 's', 't' });

            Assert.False(ReferenceEquals(string1, string2));
            Assert.That(string1.CompareTo(string2) == 0);

            Assert.That(list.Add(string1));
            Assert.That(list.Remove(string2));
        }

        #endregion

        #region Clear

        [Test]
        public void Clear_EmptyCollection_Empty()
        {
            var list = CreateEmptyList<int>();
            Assert.That(list.IsEmpty);
            list.Clear();
            Assert.That(list.IsEmpty);
        }

        [Test]
        public void Clear_NonEmptyCollection_Empty()
        {
            var list = CreateEmptyList<int>();
            var count = RandomCount();
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
