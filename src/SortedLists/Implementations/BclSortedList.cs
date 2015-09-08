namespace SortedLists
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using C5;

    public class BclSortedList<T> : ISortedList<T> where T : IComparable<T>
    {
        private readonly SortedList<T, object> _list;
        
        public BclSortedList() { _list = new SortedList<T, object>(); }
        public IEnumerator<T> GetEnumerator() { return _list.Keys.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        public int Count { get { return _list.Count; } }
        public bool IsEmpty { get { return Count == 0; } }
        public bool AllowsDuplicates { get { return false; } }
        public Speed IndexingSpeed { get { throw new NotImplementedException(); } }
        public T this[int i] { get { throw new NotImplementedException(); } }
        public T First { get { throw new NotImplementedException(); } }
        public T Last { get { throw new NotImplementedException(); } }
        public IEnumerable<T> EnumerateFromIndex(int index) { return EnumerateRange(index, Count); }
        public IEnumerable<T> EnumerateRange(int inclusiveFrom, int exclusiveTo) { while (inclusiveFrom < exclusiveTo)yield return this[inclusiveFrom++]; }
        public IEnumerable<T> EnumerateBackwards() { return IsEmpty ? Enumerable.Empty<T>() : EnumerateBackwardsFromIndex(Count - 1); }
        public IEnumerable<T> EnumerateBackwardsFromIndex(int index) { while (index >= 0)yield return this[index--]; }
        public int IndexOf(T item) { throw new NotImplementedException(); }
        public bool Contains(T item) { return _list.ContainsKey(item); }

        public bool Add(T item)
        {
            if (_list.ContainsKey(item))
                return false;
            _list.Add(item, null);
            return true;
        }
        public bool Remove(T item) { return _list.Remove(item); }
        public void Clear() { _list.Clear(); }
    }
}
