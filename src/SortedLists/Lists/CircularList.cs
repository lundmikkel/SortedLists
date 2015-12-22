namespace SortedLists.Lists
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;

    public class CircularList<T> : IList<T>
    {
        #region Fields

        private const int MinCapacity = 8;

        private T[] _array;
        private int _head, _tail, _count;

        #endregion


        #region Code Contracts

        [ContractInvariantMethod]
        private void invariants()
        {
            // Head and tail must be with each their bounds
            Contract.Invariant(0 <= _head && _head < _array.Length);
            Contract.Invariant(0 < _tail && _tail <= _array.Length);
            // In an empty list, head and tail must be the same
            Contract.Invariant(!IsEmpty || _head == _tail);
            // In a full list, head and tail must either be the same, or at each their end
            Contract.Invariant(_count != _array.Length || _head == _tail || _head == 0 && _tail == _array.Length);
            // Count must be equal to the (wrapped) distance between head and tail
            Contract.Invariant(_head == _tail || _head < _tail ? _count == _tail - _head : _count == _array.Length - _tail + _head);

            // Count is at most the size of the array
            Contract.Invariant(_count <= _array.Length);

            // Array is never null
            Contract.Invariant(_array != null);
            // Array is never smaller than minimum limit
            Contract.Invariant(_array.Length >= MinCapacity);
        }

        #endregion


        #region Constructors

        public CircularList(int capacity = MinCapacity)
        {
            // Create new array no smaller than the minimum limit
            _array = new T[Math.Min(capacity, MinCapacity)];
            _head = _tail = _count = 0;
        }

        #endregion


        #region IEnumerable<T>

        public IEnumerator<T> GetEnumerator()
        {
            // If head is before tail, elements are consecutive
            if (_head < _tail)
            {
                for (var i = _head; i < _tail; ++i)
                    yield return _array[i];
            }
            else
            {
                // Iterate the list's first items from the end of the array
                for (var i = _head; i < _array.Length; ++i)
                    yield return _array[i];
                // Iterate the list's last items from the beginning of the array
                for (var i = 0; i < _tail; ++i)
                    yield return _array[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion


        #region IList<T>

        public int Count { get { return _count; } }

        public bool IsReadOnly { get { return false; } }

        public T this[int index]
        {
            get
            {
                Contract.Requires(0 <= index && index < Count);

                return _array[convert(index)];
            }
            set
            {
                Contract.Requires(0 <= index && index < Count);

                _array[convert(index)] = value;
            }
        }

        public void Add(T item)
        {
            // Ensure we have enough room
            if (_count == _array.Length)
                resize();

            // Save index and advance tail
            var index = _tail++;
            // Wrap if necessary
            if (index == _array.Length)
                index = 0;

            // Insert item
            _array[index] = item;

            ++_count;
        }

        public bool Remove(T item)
        {
            // Get the index of the item
            var index = IndexOf(item);

            // If item exists, remove it
            if (index >= 0)
            {
                RemoveAt(index);
                return true;
            }

            // Otherwise fail
            return false;
        }

        public void Insert(int index, T item)
        {
            if (_head < _tail)
            {
                var mid = _head + _count / 2;

            }

            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            Contract.Requires(0 <= index && index < Count);
            // Count is decremented by one
            Contract.Ensures(Count == Contract.OldValue(Count - 1));
            // Element at index is removed
            Contract.Ensures(Enumerable.SequenceEqual(this, Contract.OldValue(this.Take(index).Concat(this.Skip(index + 1)).ToArray())));

            // If head is before tail, elements are consecutive
            if (_head < _tail)
            {
                // Index is closer to head
                if (index < _count/2)
                {
                    // Move all items from head to index one place forward
                    Array.Copy(_array, _head, _array, _head + 1, index);
                    // Delete item at old head
                    _array[_head] = default(T);
                    // Advance head
                    ++_head;
                }
                // Index ia closer to tail
                else
                {
                    // Move all items after index to tail one place backwards
                    Array.Copy(_array, _head + index + 1, _array, _head + index, _tail - index - 1);
                    // Move tail back
                    --_tail;
                    // Delete item at old tail
                    _array[_tail] = default(T);
                }
            }
            else
            {
                var headCount = _array.Length - _head;

                // Index is closer to head
                if (index < _count / 2)
                {
                    // TODO: if ()

                    // Move all items from head to index one place forward
                    Array.Copy(_array, _head, _array, _head + 1, index);
                    // Delete item at old head
                    _array[_head] = default(T);
                    // Advance head
                    ++_head;
                }
                // Index ia closer to tail
                else
                {
                    // Move all items after index to tail one place backwards
                    Array.Copy(_array, _head + index + 1, _array, _head + index, _tail - index - 1);
                    // Move tail back
                    --_tail;
                    // Delete item at old tail
                    _array[_tail] = default(T);
                }
            }


            // If list becomes empty, reset pointers
            if (_head == _tail)
                _head = _tail = 0;

            // Decrement count
            _count--;

            // If array is only a quater full, resize
            if (_count <= _array.Length / 4)
                resize();
        }

        public void Clear()
        {
            _array = new T[MinCapacity];
            _head = _tail = _count = 0;
        }

        public int IndexOf(T item)
        {
            // If head is before tail, elements are consecutive
            if (_head < _tail)
            {
                for (var i = _head; i < _tail; ++i)
                    if (item.Equals(_array[i]))
                        return i - _head;
            }
            else
            {
                // Iterate the list's first items from the end of the array
                for (var i = _head; i < _array.Length; ++i)
                    if (item.Equals(_array[i]))
                        return i - _head;
                // Iterate the list's last items from the beginning of the array
                for (var i = 0; i < _tail; ++i)
                    if (item.Equals(_array[i]))
                        return i + _array.Length - _head;
            }

            return -1;
        }

        public bool Contains(T item)
        {
            return IndexOf(item) > 0;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            // If head is before tail, elements are consecutive
            if (_head < _tail)
            {
                for (var i = _head; i < _tail; ++i)
                    array[arrayIndex++] = _array[i];
            }
            else
            {
                // Copy the list's first items from the end of the array
                for (var i = _head; i < _array.Length; ++i)
                    array[arrayIndex++] = _array[i];
                // Copy the list's last items from the beginning of the array
                for (var i = 0; i < _tail; ++i)
                    array[arrayIndex++] = _array[i];
            }
        }

        #endregion


        #region Methods

        public bool IsEmpty { get { return _count == 0; } }

        private int convert(int index)
        {
            var i = index + _head;
            return i >= _array.Length ? i - _array.Length : i;
        }

        private void resize()
        {
            var newCapacity = Math.Max(_count * 2, MinCapacity);

            // If the new capacity matches the old, nothing needs to be done
            if (newCapacity == _array.Length)
                return;

            // Create new array no smaller than the minimum limit
            var newArray = new T[newCapacity];

            // If head is before tail, elements are consecutive
            if (_head < _tail)
                Array.Copy(_array, _head, newArray, 0, _count);
            else
            {
                var headCount = _array.Length - _head;
                // Move the list's first items from the end of the array
                Array.Copy(_array, _head, newArray, 0, headCount);
                // Move the list's last items from the beginning of the array
                Array.Copy(_array, 0, newArray, headCount, _count - headCount);
            }

            // Reset state
            _head = 0;
            _tail = _count;
            _array = newArray;
        }

        public void moveHead(int positionsToTheRight, int count)
        {
            Contract.Requires(0 <= count && count <= Count);
            // TODO: Contract.Requires(count + _count <= _array.Length);
            // TODO: Bound positions to the right

            // Nothing to do
            if (positionsToTheRight == 0)
                return;

            if (count == 0)
            {
                // TODO
            }

            // Move left - create space for items
            if (positionsToTheRight < 0)
            {
                // The number of items to move left at the end of the array
                var moveCount = Math.Min(count, _array.Length - _head);

                // Move head items to the left
                Array.Copy(_array, _head, _array, _head + positionsToTheRight, moveCount);

                // Update head
                _head += positionsToTheRight;

                // Subtract the number of moved items from the count
                count -= moveCount;

                // Stop if all items have been moved
                if (count <= 0)
                    return;

                // Compute temperary head
                var tmpHead = _head + moveCount;
                // Compute the number of items to move from the beginning to the end of the array
                moveCount = Math.Min(count, -positionsToTheRight);

                // Move items from the beginning of the array to the end
                Array.Copy(_array, 0, _array, tmpHead, moveCount);

                // Subtract the number of moved items from the count
                count -= moveCount;

                // Stop if all items have been moved
                if (count <= 0)
                    return;


                // Move the last items to the beginning of the array
                Array.Copy(_array, -positionsToTheRight, _array, 0, count);
            }
            // Move right - remove items
            else
            {
                // The number of items to move left at the end of the array
                var headCount = Math.Min(count, _array.Length - _head);

                // Check if items at the beginning needs to be moved
                if (headCount < count)
                {
                    // Move items at the beginning of the array to the right
                    Array.Copy(_array, 0, _array, count + positionsToTheRight, count - headCount);
                }

                if (_head + headCount + positionsToTheRight > _array.Length)
                {
                    // Move items from the beginning to the end
                    Array.Copy(_array, _array.Length - positionsToTheRight, _array, 0, positionsToTheRight);
                }

                var moveCount = count; //headCount - positionsToTheRight;

                // Move items at head to the right
                Array.Copy(_array, _head, _array, _head + positionsToTheRight, moveCount);
                
                // Clear array where items were moved
                Array.Clear(_array, _head, positionsToTheRight);

                // Update head
                _head += positionsToTheRight;
            }

            _count -= positionsToTheRight;
        }

        // TODO: Prepend method

        #endregion
    }
}
