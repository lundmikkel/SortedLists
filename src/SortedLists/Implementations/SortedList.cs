namespace SortedLists
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using C5;
    using SCG = System.Collections.Generic;

    public class SortedList<T> : ISortedList<T>
        where T : IComparable<T>
    {
        #region Fields

        private readonly SCG.IList<T> _list;
        private readonly bool _allowsDuplicates;

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariants()
        {
            // List is sorted
            Contract.Invariant(_allowsDuplicates ? _list.IsSorted() : _list.IsStrictlySorted());
        }

        #endregion

        #region Constructors

        public SortedList(bool allowsDuplicates = false)
        {
            _allowsDuplicates = allowsDuplicates;
            _list = CreateEmptyList();
        }

        protected virtual SCG.IList<T> CreateEmptyList()
        {
            return new List<T>();
        } 

        #endregion

        #region ISortedList<T>

        #region Properties

        /// <inheritdoc/>
        public int Count { get { return _list.Count; } }

        /// <inheritdoc/>
        public bool IsEmpty { get { return _list.Count == 0; } }

        /// <inheritdoc/>
        public bool AllowsDuplicates { get { return _allowsDuplicates; } }

        /// <inheritdoc/>
        public Speed IndexingSpeed { get { return Speed.Constant; } }

        /// <inheritdoc/>
        public T this[int i] { get { return _list[i]; } }

        /// <inheritdoc/>
        public T First { get { return _list[0]; } }

        /// <inheritdoc/>
        public T Last { get { return _list[Count - 1]; } }

        #endregion

        #region Find

        /// <inheritdoc/>
        public int IndexOf(T item)
        {
            var low = 0;
            var high = _list.Count - 1;

            while (low <= high)
            {
                var mid = low + (high - low >> 1);
                var compareTo = _list[mid].CompareTo(item);

                if (compareTo < 0)
                    low = mid + 1;
                else if (compareTo > 0)
                    high = mid - 1;
                //Equal but range is not fully scanned
                else if (low != mid)
                    //Set upper bound to current number and rescan
                    high = mid;
                //Equal and full range is scanned
                else
                    return mid;
            }

            // key not found. return insertion point
            return ~low;
        }

        /// <inheritdoc/>
        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        #endregion

        #region Enumerable

        /// <inheritdoc/>
        public IEnumerable<T> EnumerateFromIndex(int index)
        {
            return EnumerateRange(index, Count);
        }

        /// <inheritdoc/>
        public IEnumerable<T> EnumerateRange(int inclusiveFrom, int exclusiveTo)
        {
            while (inclusiveFrom < exclusiveTo)
                yield return _list[inclusiveFrom++];
        }

        /// <inheritdoc/>
        public IEnumerable<T> EnumerateBackwards()
        {
            return IsEmpty ? Enumerable.Empty<T>(): EnumerateBackwardsFromIndex(Count - 1);
        }

        /// <inheritdoc/>
        public IEnumerable<T> EnumerateBackwardsFromIndex(int index)
        {
            while (index >= 0)
                yield return _list[index--];
        }

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator() { return _list.GetEnumerator(); }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        #endregion

        #region Extensible

        /// <inheritdoc/>
        public bool Add(T item)
        {
            var index = LastIndexOf(item);

            if (!AllowsDuplicates && index >= 0)
                return false;

            index = (index < 0) ? ~index : index + 1;
            _list.Insert(index, item);

            return true;
        }

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            var index = LastIndexOf(item);

            if (index < 0)
                return false;

            _list.RemoveAt(index);
            return true;
        }

        private int LastIndexOf(T item)
        {
            Contract.Requires(item != null);

            Contract.Ensures(0 <= Contract.Result<int>()
                ? this[Contract.Result<int>()].CompareTo(item) == 0 && (Count <= Contract.Result<int>() + 1 || item.CompareTo(this[Contract.Result<int>() + 1]) < 0)
                : (!(0 <= ~Contract.Result<int>() - 1 && ~Contract.Result<int>() - 1 < Count) || this[~Contract.Result<int>() - 1].CompareTo(item) < 0)
                && (!(0 <= ~Contract.Result<int>() && ~Contract.Result<int>() < Count) || item.CompareTo(this[~Contract.Result<int>()]) < 0));

            var low = 0;
            var high = _list.Count - 1;
            var found = false;

            while (low <= high)
            {
                var mid = low + (high - low >> 1);
                var compareTo = _list[mid].CompareTo(item);

                if (compareTo < 0)
                    low = mid + 1;
                else if (compareTo > 0)
                    high = mid - 1;
                else {
                    // TODO: Find a better solution
                    found = true;
                    low = mid + 1;
                }
            }

            return found ? high : ~low;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _list.Clear();
        }

        #endregion

        #endregion

        #region Methods
        public override string ToString()
        {
            return _list.ToString();
        }
        #endregion
    }
}
