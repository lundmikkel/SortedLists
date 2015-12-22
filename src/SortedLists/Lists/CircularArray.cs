namespace SortedLists.Lists
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    public class CircularArray<T> : IEnumerable<T>
    {
        #region Fields

        private readonly T[] _array;
        private int _start, _end, _length;

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void invariants()
        {
            Contract.Invariant(_start <= _end);
        }

        #endregion

        public CircularArray(int capacity)
        {
            Contract.Requires(capacity >= 0);
            Contract.Ensures(_array != null);

            _length = capacity;
            _array = new T[capacity];
            _start = _end = 0;
        }

        public int Count
        {
            get { return _end - _start; }
        }

        public int Capacity { get { return _length; } }

        public IEnumerator<T> GetEnumerator()
        {
            for (var i = _start; i < _end; i++)
                yield return _array[i % _length];
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T this[int index]
        {
            get { return _array[(_start + index) % _length]; }
        }

        internal void Insert(int index, T item)
        {
            Contract.Requires(Count < Capacity);

            // Insert at the end
            if (index == Count)
            {
                _array[_end++ % _length] = item;
            }
            else
            {
                // Move elements over to make space
                var start = (_start + index) % _length;
                var end = _end % _length;

                if (start < end)
                {
                    var length = end - start;

                    // TODO: Ensure we don't exceed array
                    Array.Copy(_array, start, _array, start + 1, length);
                }
            }

            // TODO
        }

        internal void Move(CircularArray<T> source, int start, int length)
        {
            throw new System.NotImplementedException();
        }

        internal void RemoveAt(int index)
        {
            throw new System.NotImplementedException();

            _array[_end % _length] = default(T);
        }

        internal void AddRange(CircularArray<T> list)
        {
            Contract.Requires(list.Count <= Capacity - Count);

            // TODO: Optimize
            foreach (var item in list)
                _array[_end++ % _length] = item;
        }
    }
}