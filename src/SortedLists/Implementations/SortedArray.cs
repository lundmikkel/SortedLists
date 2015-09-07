namespace SortedLists
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using C5;

    public class SortedArray<T> : ISortedList<T> where T : IComparable<T>
    {
        private readonly C5.SortedArray<T> _list;

        public SortedArray() { _list = new C5.SortedArray<T>(); }
        public IEnumerator<T> GetEnumerator() { return _list.GetEnumerator(); }
        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        public int Count { get { return _list.Count; } }
        public bool IsEmpty { get { return _list.IsEmpty; } }
        public bool AllowsDuplicates { get { return _list.AllowsDuplicates; } }
        public Speed IndexingSpeed { get { return _list.IndexingSpeed; } }
        public T this[int i] { get { return _list[i]; } }
        public T First { get { return _list[0]; } }
        public T Last { get { return _list[Count - 1]; } }
        public IEnumerable<T> EnumerateFromIndex(int index) { return EnumerateRange(index, Count); }
        public IEnumerable<T> EnumerateRange(int inclusiveFrom, int exclusiveTo) { while (inclusiveFrom < exclusiveTo)yield return _list[inclusiveFrom++]; }
        public IEnumerable<T> EnumerateBackwards() { return IsEmpty ? Enumerable.Empty<T>() : EnumerateBackwardsFromIndex(Count - 1); }
        public IEnumerable<T> EnumerateBackwardsFromIndex(int index) { while (index >= 0)yield return _list[index--]; }
        public int IndexOf(T item) { return _list.IndexOf(item); }
        public bool Contains(T item) { return _list.Contains(item); }
        public bool Add(T item) { return _list.Add(item); }
        public bool Remove(T item) { return _list.Remove(item); }
        public void Clear() { _list.Clear(); }
    }
}
