namespace SortedLists.Tests.InterfaceTests
{
    using System;
    using C5;
    using Interfaces;
    using NUnit.Framework;

    [TestFixture]
    public abstract class SortedListTestBase
    {
        #region Fields

        protected Random Random;

        #endregion
        protected abstract ISortedList<int> CreateEmptyList();

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
            var list = CreateEmptyList();
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
                return;
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
            var list = CreateEmptyList();
            Assert.AreEqual(0, list.Count);
        }

        [Test]
        public void Count_NonEmptyCollection_Count()
        {
            var list = CreateEmptyList();
            var count = Random.Next(10, 20);

            for (var i = 1; i <= count; ++i)
            {
                list.Add(i);
                Assert.AreEqual(i, list.Count);
            }
        }

        #endregion

        #region IsEmpty

        [Test]
        public void IsEmpty_EmptyCollection_True()
        {
            var list = CreateEmptyList();
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
            var list = CreateEmptyList();
            Assert.AreEqual(AllowsDuplicates(), list.AllowsDuplicates);
        }

        #endregion

        #region IndexingSpeed

        [Test]
        public void IndexingSpeed_Collection_NotInfinite()
        {
            var list = CreateEmptyList();
            Assert.AreNotEqual(Speed.PotentiallyInfinite, list.IndexingSpeed);
        }

        #endregion

        #region Indexer

        [Test]
        public void Indexer_EmptyCollection_AnyIndexThrowsContractException()
        {
            var list = CreateEmptyList();

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
            var list = CreateEmptyList();
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
            var list = CreateEmptyList();
            AssertThrowsContractException(() => { var dummy = list.First; });
        }

        [Test]
        public void First_NonEmptyCollection_FirstElement()
        {
            var list = CreateEmptyList();
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
            var list = CreateEmptyList();
            AssertThrowsContractException(() => { var dummy = list.Last; });
        }

        [Test]
        public void Last_NonEmptyCollection_LastElement()
        {
            var list = CreateEmptyList();
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

        #endregion
    }
}
