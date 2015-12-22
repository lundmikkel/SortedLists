namespace SortedLists.Lists
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class JavaCircularArrayList<T>
    {
        private T[] _array;

        // _head points to the first logical element in the array
        private int _head;
        // _tail points to the element following the last
        private int _tail;

        // This means that the list is empty when _head == _tail. 
        // It also means that the _array array has to have an extra space in it.
        // Strictly speaking, we don't need to keep a handle to _count,
        // as it can be calculated programmatically, but keeping it
        // makes the algorithms faster.
        private int _count;

        private int _version;

        // TODO: Use a sensable start value
        public JavaCircularArrayList()
            : this(10)
        {
        }

        public JavaCircularArrayList(int capacity)
        {
            _array = new T[capacity];
            _count = 0;
        }

        public JavaCircularArrayList(ICollection<T> collection)
        {
            _count = _tail = collection.Count;
            _array = new T[_count];
            collection.CopyTo(_array, 0);
        }

        /// <summary>
        /// Computes the actual index from the logical index (as if _head was always 0)
        /// within _array.
        /// </summary>
        /// <param name="index">The logical index</param>
        /// <returns>The actual index.</returns>
        private int convert(int index)
        {
            return (index + _head) % _array.Length;
        }

        public bool isEmpty()
        {
            return _head == _tail; // or _count == 0
        }

        // We use this method to ensure that the capacity of the
        // list will suffice for the number of elements we want to
        // insert.  If it is too small, we make a new, bigger array
        // and copy the old elements in.
        public void ensureCapacity(int minCapacity)
        {
            var oldCapacity = _array.Length;

            if (minCapacity > oldCapacity)
            {
                var newCapacity = (oldCapacity * 3) / 2 + 1;
                if (newCapacity < minCapacity)
                    newCapacity = minCapacity;

                var newData = new T[newCapacity];
                // TODO!
                
                _array = newData;
                _tail = _count;
                _head = 0;
            }
        }

        public int Count()
        {
            // the _count can also be worked out each time as:
            // (_tail + _array.Length - _head) % _array.Length
            return _count;
        }

        public bool Contains(T elem)
        {
            return IndexOf(elem) >= 0;
        }

        public int IndexOf(T elem)
        {
            if (elem == null)
            {
                for (var i = 0; i < _count; ++i)
                    if (_array[convert(i)] == null)
                        return i;
            }
            else
            {
                for (int i = 0; i < _count; i++)
                    if (elem.Equals(_array[convert(i)]))
                        return i;
            }
            return -1;
        }

        public int lastIndexOf(Object elem)
        {
            if (elem == null)
            {
                for (int i = _count - 1; i >= 0; i--)
                    if (_array[convert(i)] == null)
                        return i;
            }
            else
            {
                for (int i = _count - 1; i >= 0; i--)
                    if (elem.Equals(_array[convert(i)]))
                        return i;
            }
            return -1;
        }

        public T[] ToArray()
        {
            throw new NotImplementedException();
            //return ToArray(new T[_count]);
        }


        private void rangeCheck(int index)
        {
            if (index >= _count || index < 0)
                throw new Exception("Index: " + index + ", Size: " + _count);
        }

        public Object get(int index)
        {
            rangeCheck(index);
            return _array[convert(index)];
        }

        public Object set(int index, Object element)
        {
            throw new NotImplementedException();
            _version++;
            rangeCheck(index);
            Object oldValue = _array[convert(index)];
            //_array[convert(index)] = element;
            return oldValue;
        }

        public bool add(Object o)
        {
            throw new NotImplementedException();
            _version++;
            // We have to have at least one empty space
            ensureCapacity(_count + 1 + 1);
            //_array[_tail] = o;
            _tail = (_tail + 1) % _array.Length;
            _count++;
            return true;
        }

        // This method is the main reason we re-wrote the class.
        // It is optimized for removing first and last elements
        // but also allows you to remove in the middle of the list.
        public Object remove(int index)
        {
            _version++;
            rangeCheck(index);
            int pos = convert(index);
            // an interesting application of try/finally is to avoid
            // having to use local variables
            try
            {
                return _array[pos];
            }
            finally
            {
                throw new NotImplementedException();
                //_array[pos] = null; // Let gc do its work
                // optimized for FIFO access, i.e. adding to back and
                // removing from front
                if (pos == _head)
                {
                    _head = (_head + 1) % _array.Length;
                }
                else if (pos == _tail)
                {
                    _tail = (_tail - 1 + _array.Length) % _array.Length;
                }
                else
                {
                    if (_head < pos && _tail < pos)
                    { // _tail/_head/pos
                        Array.Copy(_array, _head, _array, _head + 1, pos - _head);
                        _head = (_head + 1) % _array.Length;
                    }
                    else
                    {
                        Array.Copy(_array, pos + 1, _array, pos, _tail - pos - 1);
                        _tail = (_tail - 1 + _array.Length) % _array.Length;
                    }
                }
                _count--;
            }
        }

        public void clear()
        {
            throw new NotImplementedException();
            _version++;
            // Let gc do its work
            for (int i = _head; i != _tail; i = (i + 1) % _array.Length)
                //_array[i] = null;
            _head = _tail = _count = 0;
        }

        public bool addAll(ICollection<T> c)
        {
            throw new NotImplementedException();
            _version++;
            int numNew = c.Count();
            // We have to have at least one empty space
            ensureCapacity(_count + numNew + 1);
            var e = c.GetEnumerator();
            for (int i = 0; i < numNew; i++)
            {
                //_array[_tail] = e.Current();
                _tail = (_tail + 1) % _array.Length;
                _count++;
            }
            return numNew != 0;
        }

        public void add(int index, Object element)
        {
            throw new NotImplementedException();
        }

        public bool addAll(int index, ICollection<T> c)
        {
            throw new NotImplementedException();
        }
    }
}