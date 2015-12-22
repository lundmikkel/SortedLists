namespace SortedLists
{
    using System;
    using System.Collections.Generic;
    using System.Collections;
    using C5;

    public class SortedSet<T> : ISortedList<T> where T : IComparable<T>
    {
        private System.Collections.Generic.SortedSet<T> _set;

        public SortedSet()
        {
            _set = new System.Collections.Generic.SortedSet<T>();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _set.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int Count { get { return _set.Count; } }
        public bool IsEmpty { get { return _set.Count == 0; } }
        public bool AllowsDuplicates { get { return false; } }
        public Speed IndexingSpeed { get { return Speed.Log; } }

        public T this[int i]
        {
            get { throw new NotImplementedException(); }
        }

        public T First { get { return _set.Min; } }
        public T Last { get { return _set.Max; } }
        public IEnumerable<T> EnumerateFromIndex(int index)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> EnumerateRange(int inclusiveFrom, int exclusiveTo)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> EnumerateBackwards()
        {
            return _set.Reverse();
        }

        public IEnumerable<T> EnumerateBackwardsFromIndex(int index)
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            return _set.Contains(item);
        }

        public bool Add(T item)
        {
            return _set.Add(item);
        }

        public bool Remove(T item)
        {
            return _set.Remove(item);
        }

        public void Clear()
        {
            _set.Clear();
        }
    }
}
