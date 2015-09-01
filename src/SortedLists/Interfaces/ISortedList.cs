namespace SortedLists.Interfaces
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using C5;

    [ContractClass(typeof(SortedListContract<>))]
    public interface ISortedList<T> : IEnumerable<T> where T : IComparable<T>
    {
        #region Properties

        [Pure]
        int Count { get; }

        [Pure]
        bool IsEmpty { get; }

        [Pure]
        bool AllowsDuplicates { get; }

        [Pure]
        Speed IndexingSpeed { get; }

        [Pure]
        T this[int i] { get; }

        [Pure]
        T First { get; }

        [Pure]
        T Last { get; }

        #endregion

        #region Enumerable

        [Pure]
        IEnumerable<T> EnumerateFromIndex(int index);

        [Pure]
        IEnumerable<T> EnumerateRange(int inclusiveFrom, int exclusiveTo);

        [Pure]
        IEnumerable<T> EnumerateBackwardsFromIndex(int index);

        #endregion

        #region Find

        int IndexOf(T item);

        bool Contains(T item);

        #endregion

        #region Extensible

        bool Add(T item);

        bool Remove(T item);

        void Clear();

        #endregion
    }

    [ContractClassFor(typeof(ISortedList<>))]
    abstract class SortedListContract<T> : ISortedList<T>
        where T : IComparable<T>
    {
        #region Properties

        public int Count
        {
            get
            {
                Contract.Ensures(Contract.Result<int>() >= 0);
                Contract.Ensures(this.Any() ? Contract.Result<int>() > 0 : Contract.Result<int>() == 0);
                Contract.Ensures(Enumerable.Count(this) == Contract.Result<int>());

                throw new NotImplementedException();
            }
        }

        public bool IsEmpty
        {
            get
            {
                Contract.Ensures(Contract.Result<bool>() == (Count == 0));

                throw new NotImplementedException();
            }
        }

        public bool AllowsDuplicates
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Speed IndexingSpeed
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public T this[int i]
        {
            get
            {
                // Requires index be in bounds
                Contract.Requires(0 <= i && i < Count);

                // If the index is correct, the output can never be null
                Contract.Ensures(Contract.Result<T>() != null);
                // Result is the same as skipping the first i elements
                Contract.Ensures(ReferenceEquals(Contract.Result<T>(), this.Skip(i).First()));

                throw new NotImplementedException();
            }
        }

        public T First
        {
            get
            {
                Contract.Requires(Count > 0);

                // Result is the same 
                Contract.Ensures(ReferenceEquals(Contract.Result<T>(), this[0]));

                throw new NotImplementedException();
            }
        }

        public T Last
        {
            get
            {
                Contract.Requires(Count > 0);

                // Result is the same as this[Count - 1]
                Contract.Ensures(ReferenceEquals(Contract.Result<T>(), this[Count - 1]));

                throw new NotImplementedException();
            }
        }
        
        #endregion

        #region Enumerable

        public IEnumerable<T> EnumerateFromIndex(int index)
        {
            Contract.Requires(0 <= index && index < Count);

            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            Contract.Ensures(Contract.Result<IEnumerable<T>>().IsSorted());

            // Enumerable is the same as skipping the first index intervals
            Contract.Ensures(Enumerable.SequenceEqual(
                this.Skip(index),
                Contract.Result<IEnumerable<T>>()
            ));

            throw new NotImplementedException();
        }

        public IEnumerable<T> EnumerateRange(int inclusiveFrom, int exclusiveTo)
        {
            Contract.Requires(0 <= inclusiveFrom && inclusiveFrom < Count);
            Contract.Requires(1 <= exclusiveTo && exclusiveTo <= Count);
            Contract.Requires(inclusiveFrom < exclusiveTo);

            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            // The enumerator is endpoint sorted
            Contract.Ensures(Contract.Result<IEnumerable<T>>().IsSorted());
            // Enumerable is the same as skipping the first inclusiveFrom intervals and then tale t
            Contract.Ensures(Enumerable.SequenceEqual(
                this.Skip(inclusiveFrom).Take(exclusiveTo - inclusiveFrom),
                Contract.Result<IEnumerable<T>>()
            ));

            throw new NotImplementedException();
        }

        public IEnumerable<T> EnumerateBackwardsFromIndex(int index)
        {
            Contract.Requires(0 <= index && index < Count);

            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);
            // The enumerator is endpoint sorted
            Contract.Ensures(Contract.Result<IEnumerable<T>>().IsSortedBackwards());
            // Enumerable is the same as skipping the first index intervals
            Contract.Ensures(Enumerable.SequenceEqual(
                this.Take(index + 1).Reverse(),
                Contract.Result<IEnumerable<T>>()
            ));

            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            Contract.Ensures(Count > 0 == Contract.Result<IEnumerator<T>>().ToEnumerable().Any());

            // Result is never null
            Contract.Ensures(Contract.Result<IEnumerator<T>>() != null);

            // The enumerator is endpoint sorted
            Contract.Ensures(Contract.Result<IEnumerator<T>>().ToEnumerable().IsSorted());

            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        
        #endregion

        #region Find

        public int IndexOf(T item)
        {
            Contract.Requires(item != null);

            Contract.Ensures(0 <= Contract.Result<int>()
                ? this[Contract.Result<int>()].CompareTo(item) == 0 && (Contract.Result<int>() - 1 < 0 || this[Contract.Result<int>() - 1].CompareTo(item) < 0)
                : (!(0 <= ~Contract.Result<int>() - 1) || this[~Contract.Result<int>() - 1].CompareTo(item) < 0) && (~Contract.Result<int>() < Count || item.CompareTo(this[~Contract.Result<int>()]) < 0));

            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            Contract.Requires(item != null);

            Contract.Ensures(Contract.Result<bool>() == IndexOf(item) >= 0);

            Contract.Ensures(Contract.Result<bool>() == Contract.Exists(this, x => x.CompareTo(item) == 0));

            throw new NotImplementedException();
        }
        
        #endregion

        #region Extensible

        public bool Add(T item)
        {
            Contract.Requires(item != null);

            // The collection cannot be empty afterwards
            Contract.Ensures(!IsEmpty);

            // The collection contains the item
            Contract.Ensures(Contains(item));

            // If the item is added the count goes up by one, otherwise stays the same
            Contract.Ensures(Count == Contract.OldValue(Count) + (Contract.Result<bool>() ? 1 : 0));

            // If the item was added, the number of equal items goes up by one, otherwise stays the same
            Contract.Ensures(this.Count(x => x.CompareTo(item) == 0) == Contract.OldValue(this.Count(x => x.CompareTo(item) == 0)) + (Contract.Result<bool>() ? 1 : 0));

            Contract.Ensures(AllowsDuplicates || Contract.OldValue(Contract.ForAll(this, x => x.CompareTo(item) != 0)) || !Contract.Result<bool>());

            throw new NotImplementedException();
        }

        public bool Remove(T item)
        {
            Contract.Requires(item != null);

            // Nothing to remove if the collection is empty
            Contract.Ensures(Contract.OldValue(Count > 0) || !Contract.Result<bool>());

            // If the item is removed the count goes down by one, otherwise stays the same
            Contract.Ensures(Count == Contract.OldValue(Count) - (Contract.Result<bool>() ? 1 : 0));

            // If the item was removed, the number of equal items goes down by one, otherwise stays the same
            Contract.Ensures(this.Count(x => x.CompareTo(item) == 0) == Contract.OldValue(this.Count(x => x.CompareTo(item) == 0)) - (Contract.Result<bool>() ? 1 : 0));

            // The result is true if the collection contained the interval
            Contract.Ensures(Contract.Result<bool>() == Contract.OldValue(Contract.Exists(this, x => x.CompareTo(item) == 0)));

            throw new NotImplementedException();
        }

        public void Clear()
        {
            // The collection must be empty afterwards
            Contract.Ensures(Count == 0);

            // Enumerator is empty
            Contract.Ensures(!this.Any());

            throw new NotImplementedException();
        }

        #endregion
    }
}
