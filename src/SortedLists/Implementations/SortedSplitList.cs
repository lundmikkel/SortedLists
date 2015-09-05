namespace SortedLists
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using C5;

    public class SortedSplitList<T> : ISortedList<T>
        where T : IComparable<T>
    {
        #region Fields

        private readonly List<List<T>> _lists;
        private readonly List<int> _offsets;
        private readonly int _deepness;

        private int _count;
        private int _dirtyFrom;

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariants()
        {
            // List is sorted
            Contract.Invariant(this.IsStrictlySorted());
            // Count matches the sum of counts
            Contract.Invariant(_count == _lists.Select(list => list.Count).Sum());

            // There is at least one list
            Contract.Invariant(_lists.Count > 0 && _lists[0] != null);
            // No list is longer than the maximum list length
            Contract.Invariant(Contract.ForAll(_lists, list => list.Capacity <= _deepness));
            // No consecutive lists have a joined sum smaller than half the maximum list length
            Contract.Invariant(Contract.ForAll(1, _lists.Count, i => _lists[i - 1].Count + _lists[i].Count > _deepness / 2));

            // The number of lists must match the number of offset values
            Contract.Invariant(_lists.Count == _offsets.Count);
            // The offsets are either dirty, or correct
            Contract.Invariant(offsetsAreCorrectBefore(_dirtyFrom));

            // Deepness is a power of 2
            Contract.Invariant(_deepness != 0 && (_deepness & _deepness - 1) == 0);
        }

        [Pure]
        private bool offsetsAreCorrectBefore(int count)
        {
            if (_offsets[0] != 0)
                return false;

            for (var i = 1; i < count; i++)
                if (_offsets[i] != _offsets[i - 1] + _lists[i - 1].Count)
                    return false;

            return true;
        }

        #endregion

        #region Constructors

        public SortedSplitList(int deepness = 1024)
        {
            Contract.Requires(0 < deepness);
            // Ensures _deepness is a power of two
            Contract.Ensures(_deepness != 0 && (_deepness & _deepness - 1) == 0);

            // Round to smallest power of 2, not smaller than deepness to ensure we don't waste space in lists
            _deepness = (int) Math.Pow(2, Math.Ceiling(Math.Log(deepness, 2)));

            _deepness = deepness;
            _lists = new List<List<T>> { new List<T>() };
            _offsets = new List<int> { 0 };
        }

        #endregion

        #region ISortedList<T>

        #region Properties

        /// <inheritdoc/>
        public int Count { get { return _count; } }

        /// <inheritdoc/>
        public bool IsEmpty { get { return _count == 0; } }

        /// <inheritdoc/>
        public bool AllowsDuplicates { get { return false; } }

        /// <inheritdoc/>
        public Speed IndexingSpeed { get { return Speed.Log; } }

        /// <inheritdoc/>
        public T this[int index]
        {
            get
            {
                var listIndex = getListIndexFromIndex(index);
                var itemIndex = index - _offsets[listIndex];
                return _lists[listIndex][itemIndex];
            }
        }

        /// <inheritdoc/>
        public T First { get { return _lists[0][0]; } }

        /// <inheritdoc/>
        public T Last
        {
            get
            {
                var lastList = _lists[_lists.Count - 1];
                return lastList[lastList.Count - 1];
            }
        }

        #endregion

        #region Find

        /// <inheritdoc/>
        public int IndexOf(T item)
        {
            var listIndex = getListIndex(item);
            var list = _lists[listIndex];
            var itemIndex = getItemIndex(list, item);

            updateOffsets();
            return itemIndex >= 0 ? _offsets[listIndex] + itemIndex : ~(_offsets[listIndex] + ~itemIndex);
        }

        /// <inheritdoc/>
        public bool Contains(T item)
        {
            return IndexOf(item) >= 0;
        }

        #endregion

        #region Enumerable

        /// <inheritdoc/>
        public IEnumerator<T> GetEnumerator()
        {
            foreach (var list in _lists)
                foreach (var item in list)
                    yield return item;
        }

        /// <inheritdoc/>
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }

        /// <inheritdoc/>
        public IEnumerable<T> EnumerateFromIndex(int index)
        {
            var listIndex = getListIndexFromIndex(index);
            var itemIndex = index - _offsets[listIndex];

            for (var i = listIndex; i < _lists.Count; ++i)
            {
                var list = _lists[i];
                var j = i == listIndex ? itemIndex : 0;
                var count = list.Count;
                while (j < count)
                    yield return list[j++];
            }
        }

        /// <inheritdoc/>
        public IEnumerable<T> EnumerateRange(int inclusiveFrom, int exclusiveTo)
        {
            // TODO: Implement using EnumerateFromIndex and a counter?

            var listFromIndex = getListIndexFromIndex(inclusiveFrom);
            var itemFromIndex = inclusiveFrom - _offsets[listFromIndex];
            var listToIndex = getListIndexFromIndex(exclusiveTo);
            var itemToIndex = exclusiveTo - _offsets[listToIndex];

            for (var i = listFromIndex; i <= listToIndex; ++i)
            {
                var list = _lists[i];
                var j = i == listFromIndex ? itemFromIndex : 0;
                var count = i == listToIndex ? itemToIndex : list.Count;
                while (j < count)
                    yield return list[j++];
            }
        }

        /// <inheritdoc/>
        public IEnumerable<T> EnumerateBackwards()
        {
            for (var i = _lists.Count - 1; i >= 0; --i)
            {
                var list = _lists[i];
                for (var j = list.Count - 1; j >= 0; --j)
                    yield return list[j];
            }
        }

        /// <inheritdoc/>
        public IEnumerable<T> EnumerateBackwardsFromIndex(int index)
        {
            var listIndex = getListIndexFromIndex(index);
            var itemIndex = index - _offsets[listIndex];

            for (var i = listIndex; i >= 0; --i)
            {
                var list = _lists[i];
                for (var j = i == listIndex ? itemIndex : list.Count - 1; j >= 0; --j)
                    yield return list[j];
            }
        }

        #endregion

        #region Extensible

        /// <inheritdoc/>
        public bool Add(T item)
        {
            // Find the proper position for the item
            var listIndex = getListIndex(item);
            var list = _lists[listIndex];
            var itemIndex = getItemIndex(list, item);

            // Duplicate found
            if (0 <= itemIndex)
                return false;

            itemIndex = ~itemIndex;

            // Check if list fits the item
            if (list.Count < _deepness)
            {
                list.Insert(itemIndex, item);
            }
            // Otherwise split list
            else
            {
                var mid = _deepness >> 1;

                // Create new list if we are on the last list
                // or if the next list can not accommodate the second half of the list
                if (listIndex == _lists.Count - 1 || mid <= _lists[listIndex + 1].Count)
                {
                    _lists.Insert(listIndex + 1, new List<T>()); // TODO: Set list size to begin with?
                    _offsets.Add(-1);
                }

                // Move second half to next list before inserting, to avoid resizing list above deepness
                _lists[listIndex + 1].InsertRange(0, list.GetRange(mid, mid));
                list.RemoveRange(mid, mid);

                // Insert item in the appropriate list
                if (itemIndex <= mid)
                    _lists[listIndex].Insert(itemIndex, item);
                else
                    _lists[listIndex + 1].Insert(itemIndex - mid, item);
            }

            ++_count;
            makeDirtyFrom(0);

            return true;
        }

        /// <inheritdoc/>
        public bool Remove(T item)
        {
            // Find the proper position for the item
            var listIndex = getListIndex(item);
            var list = _lists[listIndex];
            var itemIndex = getItemIndex(list, item);

            // Item was not found
            if (itemIndex < 0)
                return false;

            // Remove element from list
            list.RemoveAt(itemIndex);

            // Remove lists when empty
            if (list.Count == 0)
            {
                if (_lists.Count > 1)
                {
                    _lists.RemoveAt(listIndex);
                    _offsets.RemoveAt(_offsets.Count - 1);
                }
            }
            // Move items in current list to the end of the previous list
            else if (0 < listIndex && _lists[listIndex - 1].Count + list.Count <= _deepness / 2)
            {
                _lists[listIndex - 1].AddRange(list);
                _lists.RemoveAt(listIndex);
                _offsets.RemoveAt(_offsets.Count - 1);
            }
            // Move items in current list to the beginning of the next list
            else if (listIndex < _lists.Count - 1 && list.Count + _lists[listIndex + 1].Count <= _deepness / 2)
            {
                _lists[listIndex + 1].InsertRange(0, list);
                _lists.RemoveAt(listIndex);
                _offsets.RemoveAt(_offsets.Count - 1);
            }

            --_count;
            makeDirtyFrom(0);

            return true;
        }

        /// <inheritdoc/>
        public void Clear()
        {
            _lists.Clear();
            _lists.Add(new List<T>());

            _offsets.Clear();
            _offsets.Add(0);

            _count = 0;
            _dirtyFrom = 0;
        }

        #endregion

        #endregion

        #region Methods

        // TODO: Make binary
        private int getListIndex(T item)
        {
            Contract.Requires(item != null);
            Contract.Ensures(0 <= Contract.Result<int>() && Contract.Result<int>() < _lists.Count);

            if (_lists.Count <= 1)
                return 0;

            for (int i = 1; i < _lists.Count; i++)
            {
                var compareTo = _lists[i][0].CompareTo(item);

                if (compareTo > 0)
                    return i - 1;
            }

            return _lists.Count - 1;
        }

        private int getListIndexFromIndex(int index)
        {
            Contract.Requires(0 <= index && index < _count);

            // Result index has a list
            Contract.Ensures(0 <= Contract.Result<int>() && Contract.Result<int>() < _lists.Count && _lists[Contract.Result<int>()] != null);
            // The list at result index contains index
            Contract.Ensures(_offsets[Contract.Result<int>()] <= index && index < _offsets[Contract.Result<int>()] + _lists[Contract.Result<int>()].Count);
            // Ensures the offsets are valid from 0 to result
            Contract.Ensures(offsetsAreCorrectBefore(Contract.Result<int>() + 1));

            // TODO: update only what is necessary
            updateOffsets();

            var low = 0;
            var high = _offsets.Count - 1;

            while (low <= high)
            {
                var mid = low + (high - low >> 1);
                var compareTo = _offsets[mid].CompareTo(index);

                if (compareTo < 0)
                    low = mid + 1;
                else if (compareTo > 0)
                    high = mid - 1;
                else
                    return mid; // key found
            }

            // key not found. return insertion point
            return high;
        }

        private int getItemIndex(List<T> list, T item)
        {
            var low = 0;
            var high = list.Count - 1;

            while (low <= high)
            {
                var mid = low + (high - low >> 1);
                var compareTo = list[mid].CompareTo(item);

                if (compareTo < 0)
                    low = mid + 1;
                else if (compareTo > 0)
                    high = mid - 1;
                else
                    return mid;
            }

            return ~low;
        }

        private void makeDirtyFrom(int index)
        {
            Contract.Requires(0 <= index && index <= _lists.Count);
            Contract.Ensures(_dirtyFrom <= Contract.OldValue(_dirtyFrom));
            
            if (_dirtyFrom > index)
                _dirtyFrom = index;
        }

        private void updateOffsets()
        {
            Contract.Requires(0 <= _dirtyFrom && _dirtyFrom <= _offsets.Count);
            Contract.Ensures(offsetsAreCorrectBefore(_dirtyFrom));

            if (_dirtyFrom == _offsets.Count)
                return;

            var offset = _dirtyFrom > 0 ? _offsets[_dirtyFrom] : 0;

            while (_dirtyFrom < _offsets.Count)
            {
                _offsets[_dirtyFrom] = offset;
                offset += _lists[_dirtyFrom].Count;
                ++_dirtyFrom;
            }
        }

        public override string ToString()
        {
            // TODO
            return _lists.ToString();
        }

        #endregion
    }
}
