//Copyright 2011 Trent Tobler. All rights reserved.

//Redistribution and use in source and binary forms, with or without modification, are
//permitted provided that the following conditions are met:

//   1. Redistributions of source code must retain the above copyright notice, this list of
//      conditions and the following disclaimer.

//   2. Redistributions in binary form must reproduce the above copyright notice, this list
//      of conditions and the following disclaimer in the documentation and/or other materials
//      provided with the distribution.

//THIS SOFTWARE IS PROVIDED BY TRENT TOBLER ''AS IS'' AND ANY EXPRESS OR IMPLIED
//WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND
//FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL TRENT TOBLER OR
//CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR
//CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
//SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON
//ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF
//ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace SortedLists
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using C5;
    using SortedLists;
    using SCG = System.Collections.Generic;

    /// <summary>
    /// A sorted collection (set) data structure using b-trees.
    /// </summary>
    /// <typeparam name="T">The type of items in the collection.</typeparam>
    [DebuggerDisplay("Count = {Count}")]
    public class BTree<T> : ISortedList<T>, System.Collections.Generic.ICollection<T>
        where T : IComparable<T>
    {
        #region Fields

        private Node _root;
        private readonly Node _first;
        private readonly bool _allowsDuplicates;

        #endregion

        #region Inner Class

        [DebuggerDisplay("Count={NodeCount}/{TotalCount}, First={keys[0]}")]
        private sealed class Node
        {
            #region Fields

            private readonly T[] _keys;
            private readonly Node[] _nodes;
            private Node _parent, _next, _prev;
            public int TotalCount, NodeCount;

            #endregion

            #region Code Contracts

            [ContractInvariantMethod]
            private void ObjectInvariant()
            {
                // Simple BTree invariants
                Contract.Invariant(_keys != null);
                Contract.Invariant(0 <= NodeCount && NodeCount <= _keys.Length);
                Contract.Invariant(_nodes == null || _keys.Length == _nodes.Length);

                // Indexable BTree invariants
                Contract.Invariant(0 <= TotalCount);
            }

            #endregion

            #region Construction

            /// <summary>
            /// Initialize the first node in the BTree structure.
            /// </summary>
            public Node(int nodeCapacity)
                : this(nodeCapacity, true)
            {
            }

            private Node(int nodeCapacity, bool leaf)
            {
                _keys = new T[nodeCapacity];
                _nodes = leaf ? null : new Node[nodeCapacity];
            }

            #endregion

            #region Properties

            public bool IsRoot
            {
                get
                {
                    return _parent == null;
                }
            }

            public bool IsLeaf
            {
                get
                {
                    return _nodes == null;
                }
            }

            #endregion

            #region Methods

            /// <summary>
            /// Gets the key at the specified position.
            /// </summary>
            public T GetKey(int index)
            {
                Contract.Requires(0 <= index && index < NodeCount);

                return _keys[index];
            }

            /// <summary>
            /// Get the leaf node at the specified index in the tree defined by the specified root.
            /// </summary>
            public static Node LeafAt(Node root, ref int index)
            {
                Contract.Requires(root != null);
                Contract.Requires(root.IsRoot);
                Contract.Requires(0 <= index && index < root.TotalCount);

                Contract.Ensures(Contract.Result<Node>() != null);
                Contract.Ensures(Contract.Result<Node>().IsLeaf);
                Contract.Ensures(0 <= index && index < Contract.Result<Node>().NodeCount);

                int nodeIndex = 0;
                while (true)
                {
                    // If root is a leaf, then it is the result.
                    if (root._nodes == null)
                        return root;

                    // Scan thru the nodes in the root, until the total count exceeds the index.
                    var node = root._nodes[nodeIndex];
                    if (index < node.TotalCount)
                    {
                        // Found the node.  Move down one level.
                        root = node;
                        nodeIndex = 0;
                    }
                    else
                    {
                        // Move to the next node in the root, and adjust index to be
                        // relative to the first element in that node.
                        index -= node.TotalCount;
                        ++nodeIndex;
                    }
                }
            }

            /// <summary>
            /// Find the node and index in the tree defined by the specified root.
            /// </summary>
            public static bool Find(Node root, T key, int duplicatesBias, out Node leaf, out int index)
            {
                Contract.Requires(root != null);
                Contract.Requires(root.IsRoot);
                Contract.Ensures(Contract.ValueAtReturn<Node>(out leaf) != null);
                Contract.Ensures(0 <= Contract.ValueAtReturn<int>(out index) && Contract.ValueAtReturn<int>(out index) <= leaf.NodeCount);

                index = Array.BinarySearch(root._keys, 0, root.NodeCount, key);
                while (root._nodes != null)
                {
                    if (index >= 0)
                    {
                        if (duplicatesBias != 0)
                            MoveToDuplicatesBoundary(key, duplicatesBias, ref root, ref index);

                        // Found an exact match.  Move down one level.
                        root = root._nodes[index];
                    }
                    else
                    {
                        // No exact match.  Find greatest lower bound.
                        index = ~index;
                        if (index > 0)
                            --index;
                        root = root._nodes[index];
                    }
                    Contract.Assume(root != null);
                    index = Array.BinarySearch(root._keys, 0, root.NodeCount, key);
                }

                leaf = root;
                if (index < 0)
                {
                    index = ~index;
                    return false;
                }

                if (duplicatesBias != 0)
                    MoveToDuplicatesBoundary(key, duplicatesBias, ref leaf, ref index);

                return true;
            }

            /// <summary>
            /// Insert a new key into the leaf node at the specified position.
            /// </summary>
            public static void Insert(T key, Node leaf, int index, ref Node root)
            {
                // Make sure there is space for the new key.
                if (EnsureSpace(leaf, ref root) && index > leaf.NodeCount)
                {
                    index -= leaf.NodeCount;
                    leaf = leaf._next;
                }

                // Insert the key.
                Array.Copy(leaf._keys, index, leaf._keys, index + 1, leaf.NodeCount - index);
                leaf._keys[index] = key;
                ++leaf.NodeCount;

                // Make sure parent keys index into this node correctly.
                EnsureParentKey(leaf, index);

                // Update total counts.
                for (var node = leaf; node != null; node = node._parent)
                    ++node.TotalCount;
            }

            /// <summary>
            /// Remove the item from the node at the specified position.
            /// </summary>
            public static void Remove(Node leaf, int index, ref Node root)
            {
                Contract.Requires(leaf != null);
                Contract.Requires(0 <= index && index < leaf.NodeCount);
                Contract.Requires(leaf.IsLeaf);

                // Update total counts.
                for (var node = leaf; node != null; node = node._parent)
                    --node.TotalCount;

                // Remove the key from the node.
                --leaf.NodeCount;
                Array.Copy(leaf._keys, index + 1, leaf._keys, index, leaf.NodeCount - index);
                leaf._keys[leaf.NodeCount] = default(T);

                // Make sure parent keys index correctly into this node.
                if (leaf.NodeCount > 0)
                    EnsureParentKey(leaf, index);

                // Merge this node with others if it is below the node capacity threshold.
                Merge(leaf, ref root);
            }

            /// <summary>
            /// Get all items starting at the index, and moving forward.
            /// </summary>
            public static IEnumerable<T> EnumerateFromIndex(Node leaf, int index)
            {
                Contract.Requires(leaf != null);
                Contract.Requires(leaf.IsLeaf);
                Contract.Requires(0 <= index && index <= leaf.NodeCount);

                while (leaf != null)
                {
                    while (index < leaf.NodeCount)
                    {
                        yield return leaf.GetKey(index);
                        ++index;
                    }
                    index -= leaf.NodeCount;
                    leaf = leaf._next;
                }
            }

            /// <summary>
            /// Get all items starting at the index, and moving backward.
            /// </summary>
            public static IEnumerable<T> EnumerateBackwardsFromIndex(Node leaf, int index)
            {
                Contract.Requires(leaf != null);
                Contract.Requires(leaf.IsLeaf);
                Contract.Requires(-1 <= index && index <= leaf.NodeCount);

                if (index == -1)
                {
                    // Handle special case to start moving in the previous node.
                    leaf = leaf._prev;
                    index = leaf != null ? leaf.NodeCount - 1 : 0;
                }
                else if (index == leaf.NodeCount)
                {
                    // Handle special case to start moving in the next node.
                    if (leaf._next == null)
                        --index;
                    else
                    {
                        leaf = leaf._next;
                        index = 0;
                    }
                }

                // Loop thru collection, yielding each value in sequence.
                while (leaf != null)
                {
                    while (index >= 0)
                    {
                        yield return leaf.GetKey(index);
                        --index;
                    }
                    leaf = leaf._prev;
                    if (leaf != null)
                        index += leaf.NodeCount;
                }
            }

            /// <summary>
            /// Clear all values from the specified node.
            /// </summary>
            public static void Clear(Node firstNode)
            {
                Contract.Requires(firstNode != null);

                Array.Clear(firstNode._keys, 0, firstNode.NodeCount);
                firstNode.NodeCount = 0;
                firstNode.TotalCount = 0;

                firstNode._parent = null;
                firstNode._next = null;
            }

            /// <summary>
            /// Get the index relative to the root node, for the position in the specified leaf.
            /// </summary>
            public static int GetRootIndex(Node leaf, int index)
            {
                var node = leaf;
                var rootIndex = index;
                while (node._parent != null)
                {
                    var nodeIndex = Array.IndexOf(node._parent._nodes, node, 0, node._parent.NodeCount);
                    for (var i = 0; i < nodeIndex; ++i)
                        rootIndex += node._parent._nodes[i].TotalCount;
                    node = node._parent;
                }
                return rootIndex;
            }

            #endregion

            #region Implementation

            /// <summary>
            /// (Assumes: key is a duplicate in node at index) Move to the side on the range of duplicates,
            /// as indicated by the sign of duplicatesBias.
            /// </summary>
            /// <param name="key"></param>
            /// <param name="duplicatesBias"></param>
            /// <param name="node"></param>
            /// <param name="index"></param>
            static void MoveToDuplicatesBoundary(T key, int duplicatesBias, ref Node node, ref int index)
            {
                // Technically, we could adjust the binary search to perform most of this step, but duplicates
                // are usually unexpected.. algorithm is still O(log N), because scan include at most a scan thru two nodes
                // worth of keys, for each level.
                // Also, the binary search option would still need the ugliness of the special case for moving into the 
                // previous node; it would only be a little faster, on average, assuming large numbers of duplicates were common.

                if (duplicatesBias < 0)
                {
                    // Move backward over duplicates.
                    while (index > 0 && node._keys[index - 1].CompareTo(key) == 0)
                        --index;

                    // Special case: duplicates can span backwards into the previous node because the parent
                    // key pivot might be in the center for the duplicates.
                    if (index == 0 && node._prev != null)
                    {
                        var prev = node._prev;
                        var prevIndex = prev.NodeCount;
                        while (prevIndex > 0 && prev._keys[prevIndex - 1].CompareTo(key) == 0)
                        {
                            --prevIndex;
                        }
                        if (prevIndex < prev.NodeCount)
                        {
                            node = prev;
                            index = prevIndex;
                        }
                    }
                }
                else
                {
                    // Move forward over duplicates.
                    while (index < node.NodeCount - 1 && node._keys[index + 1].CompareTo(key) == 0)
                        ++index;
                }
            }

            static bool EnsureSpace(Node node, ref Node root)
            {
                if (node.NodeCount < node._keys.Length)
                    return false;

                EnsureParent(node, ref root);
                EnsureSpace(node._parent, ref root);

                var sibling = new Node(node._keys.Length, node._nodes == null);
                sibling._next = node._next;
                sibling._prev = node;
                sibling._parent = node._parent;

                if (node._next != null)
                    node._next._prev = sibling;
                node._next = sibling;

                var index = Array.IndexOf(node._parent._nodes, node, 0, node._parent.NodeCount);
                var siblingIndex = index + 1;

                Array.Copy(node._parent._keys, siblingIndex, node._parent._keys, siblingIndex + 1, node._parent.NodeCount - siblingIndex);
                Array.Copy(node._parent._nodes, siblingIndex, node._parent._nodes, siblingIndex + 1, node._parent.NodeCount - siblingIndex);
                ++node._parent.NodeCount;
                node._parent._nodes[siblingIndex] = sibling;

                var half = node.NodeCount / 2;
                var halfCount = node.NodeCount - half;
                Move(node, half, sibling, 0, halfCount);
                return true;
            }

            static void Move(Node source, int sourceIndex, Node target, int targetIndex, int moveCount)
            {
                Move(source._keys, sourceIndex, source.NodeCount, target._keys, targetIndex, target.NodeCount, moveCount);

                int totalMoveCount;
                if (source._nodes == null)
                {
                    totalMoveCount = moveCount;
                }
                else
                {
                    Move(source._nodes, sourceIndex, source.NodeCount, target._nodes, targetIndex, target.NodeCount, moveCount);
                    totalMoveCount = 0;
                    for (int i = 0; i < moveCount; ++i)
                    {
                        var child = target._nodes[targetIndex + i];
                        child._parent = target;
                        totalMoveCount += child.TotalCount;
                    }
                }

                source.NodeCount -= moveCount;
                target.NodeCount += moveCount;

                var sn = source;
                var tn = target;
                while (sn != null && sn != tn)
                {
                    sn.TotalCount -= totalMoveCount;
                    tn.TotalCount += totalMoveCount;
                    sn = sn._parent;
                    tn = tn._parent;
                }

                EnsureParentKey(source, sourceIndex);
                EnsureParentKey(target, targetIndex);
            }

            static void Move<TItem>(TItem[] source, int sourceIndex, int sourceTotal, TItem[] target, int targetIndex, int targetTotal, int count)
            {
                Array.Copy(target, targetIndex, target, targetIndex + count, targetTotal - targetIndex);
                Array.Copy(source, sourceIndex, target, targetIndex, count);
                Array.Copy(source, sourceIndex + count, source, sourceIndex, sourceTotal - sourceIndex - count);
                Array.Clear(source, sourceTotal - count, count);
            }

            static void EnsureParent(Node node, ref Node root)
            {
                if (node._parent != null)
                    return;

                var parent = new Node(node._keys.Length, false);
                parent.TotalCount = node.TotalCount;
                parent.NodeCount = 1;
                parent._keys[0] = node._keys[0];
                parent._nodes[0] = node;

                node._parent = parent;
                root = parent;
            }

            static void EnsureParentKey(Node node, int index)
            {
                while (index == 0 && node._parent != null)
                {
                    index = Array.IndexOf(node._parent._nodes, node, 0, node._parent.NodeCount);
                    node._parent._keys[index] = node._keys[0];
                    node = node._parent;
                }
            }

            static void Merge(Node node, ref Node root)
            {
                if (node.NodeCount == 0)
                {
                    // Handle special case: Empty node.
                    if (node._parent == null)
                        return;

                    // Remove the node from the parent nodes.
                    var index = Array.IndexOf(node._parent._nodes, node, 0, node._parent.NodeCount);
                    --node._parent.NodeCount;
                    Array.Copy(node._parent._keys, index + 1, node._parent._keys, index, node._parent.NodeCount - index);
                    Array.Copy(node._parent._nodes, index + 1, node._parent._nodes, index, node._parent.NodeCount - index);
                    node._parent._keys[node._parent.NodeCount] = default(T);
                    node._parent._nodes[node._parent.NodeCount] = null;

                    // Make sure parent (of the parent) keys link down correctly.
                    if (node._parent.NodeCount > 0)
                        EnsureParentKey(node._parent, index);

                    // Delete the node from the next/prev linked list.
                    node._prev._next = node._next;
                    if (node._next != null)
                        node._next._prev = node._prev;

                    // Merge the parent node.
                    Merge(node._parent, ref root);
                    return;
                }

                if (node._next == null)
                {
                    if (node._parent == null && node.NodeCount == 1 && node._nodes != null)
                    {
                        root = node._nodes[0];
                        root._parent = null;
                    }

                    return;
                }

                if (node.NodeCount >= node._keys.Length / 2)
                    return;

                int count = node._next.NodeCount;
                if (node.NodeCount + count > node._keys.Length)
                    count -= (node.NodeCount + count) / 2;

                Move(node._next, 0, node, node.NodeCount, count);
                Merge(node._next, ref root);
            }

            #endregion
        }

        #endregion

        #region Code Contracts

        [ContractInvariantMethod]
        private void Invariants()
        {
            Contract.Invariant(_root != null);
            Contract.Invariant(_first != null);
        }

        #endregion

        #region Construction
        /// <summary>
        /// Initializes a new BTree instance.
        /// </summary>
        /// <param name="allowsDuplicates"></param>
        /// <param name="nodeCapacity"></param>
        public BTree(bool allowsDuplicates = false, int nodeCapacity = 128)
        {
            Contract.Requires(nodeCapacity > 2);

            _allowsDuplicates = allowsDuplicates;
            _first = new Node(nodeCapacity);
            _root = _first;
        }

        #endregion

        #region ISortedList<T>

        #region Properties

        public int Count { get { return _root.TotalCount; } }

        public bool IsEmpty { get { return _root.TotalCount == 0; } }

        public bool AllowsDuplicates { get { return _allowsDuplicates; } }

        public Speed IndexingSpeed { get { return Speed.Log; } }

        public T this[int i]
        {
            get
            {
                return Node.LeafAt(_root, ref i).GetKey(i);
            }
        }

        public T First
        {
            get
            {
                // TODO: Find a faster solution
                return this[0];
            }
        }

        public T Last
        {
            get
            {
                // TODO: Find a faster solution
                return this[Count - 1];
            }
        }

        #endregion

        #region Enumerable

        /// <summary>
        /// Gets an enumerator for the collection. O(1), move next: O(1)
        /// </summary>
        /// <returns>An enumerator for the collection.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            Contract.Ensures(Contract.Result<IEnumerator<T>>() != null);

            return Node.EnumerateFromIndex(_first, 0).GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IEnumerable<T> EnumerateFromIndex(int index)
        {
            Contract.Requires(0 <= index && index <= Count); // TODO: Why <= Count?

            var node = Node.LeafAt(_root, ref index);
            return Node.EnumerateFromIndex(node, index);
        }

        public IEnumerable<T> EnumerateRange(int inclusiveFrom, int exclusiveTo)
        {
            foreach (var item in EnumerateFromIndex(inclusiveFrom))
            {
                if (inclusiveFrom++ < exclusiveTo)
                    yield return item;
                else
                    yield break;
            }
        }

        public IEnumerable<T> EnumerateBackwards()
        {
            return IsEmpty ? Enumerable.Empty<T>() : EnumerateBackwardsFromIndex(Count - 1);
        }

        public IEnumerable<T> EnumerateBackwardsFromIndex(int index)
        {
            Contract.Requires(0 <= index && index <= Count);
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            var node = Node.LeafAt(_root, ref index);
            return Node.EnumerateBackwardsFromIndex(node, index);
        }

        #endregion

        #region Find

        public int IndexOf(T item)
        {
            Node leaf;
            int index;
            var found = Node.Find(_root, item, AllowsDuplicates ? -1 : 0, out leaf, out index);
            var result = Node.GetRootIndex(leaf, index);
            return found ? result : ~result;
        }

        public bool Contains(T item)
        {
            Node leaf;
            int index;
            return Node.Find(_root, item, 0, out leaf, out index);
        }

        #endregion

        #region Extensible

        public bool Add(T item)
        {
            Contract.Requires(!IsReadOnly);

            Node leaf;
            int index;
            var found = Node.Find(_root, item, 0, out leaf, out index);
            if (found && !AllowsDuplicates)
                return false;
            Node.Insert(item, leaf, index, ref _root);
            return true;
        }

        public bool Remove(T item)
        {
            Contract.Requires(!IsReadOnly);

            Node leaf;
            int index;
            if (!Node.Find(_root, item, 0, out leaf, out index))
                return false;

            Node.Remove(leaf, index, ref _root);
            return true;
        }

        public void Clear()
        {
            Contract.Requires(!IsReadOnly);

            Node.Clear(_first);
            _root = _first;
        }

        #endregion

        #endregion

        #region ICollection<T>

        /// <summary>
        /// Gets a value indicating whether the specified value is in the collection. O(log N)
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>True if the collection contains at item with the value; Otherwise, false.</returns>
        bool SCG.ICollection<T>.Contains(T value)
        {
            return Contains(value);
        }

        void System.Collections.Generic.ICollection<T>.Add(T value)
        {
            Add(value);
        }

        /// <summary>
        /// Removes the specified key value from the collection. O(log N)
        /// </summary>
        /// <param name="value">The key value to remove.</param>
        /// <returns>True if the value was added; otherwise, false.</returns>
        bool SCG.ICollection<T>.Remove(T value)
        {
            return Remove(value);
        }

        #endregion

        #region Properties
        int System.Collections.Generic.ICollection<T>.Count { get { return _root.TotalCount; } }

        /// <summary>
        /// Gets or sets indication whether this collection is readonly or mutable.
        /// </summary>
        public bool IsReadOnly { get; set; }


        #endregion

        #region Methods

        /// <summary>
        /// Removes the item at the specified index. O(log N)
        /// </summary>
        /// <param name="index">The index from which to remove.</param>
        public void RemoveAt(int index)
        {
            Contract.Requires(0 <= index && index < Count);
            Contract.Requires(!IsReadOnly);

            var leaf = Node.LeafAt(_root, ref index);
            Node.Remove(leaf, index, ref _root);
        }

        /// <summary>
        /// Copies the collection into the specified array. O(N)
        /// </summary>
        /// <param name="array">The array into which to copy.</param>
        /// <param name="arrayIndex">The index at which to start copying.</param>
        public void CopyTo(T[] array, int arrayIndex)
        {
            Contract.Requires(array != null);
            Contract.Requires(arrayIndex + Count <= array.Length);

            foreach (var item in this)
                array[arrayIndex++] = item;
        }

        /// <summary>
        /// Gets the index of the first item greater than the specified value. O(log N), move next: O(1)
        /// </summary>
        /// <param name="value">The value for which to find the index.</param>
        /// <returns>The index of the first item greater than the specified value, or Count if no such item exists.</returns>
        public int IndexOfFirstGreaterThan(T value)
        {
            Contract.Ensures(Contract.Result<int>() >= 0 && Contract.Result<int>() <= Count);

            Node leaf;
            int index;
            var found = Node.Find(_root, value, AllowsDuplicates ? 1 : 0, out leaf, out index);
            var result = Node.GetRootIndex(leaf, index);
            return found ? result + 1 : result;
        }

        /// <summary>
        /// Gets the index of the last item less than the specified key. O(log N), move next: O(1)
        /// </summary>
        /// <param name="value">The value for which to find the index.</param>
        /// <returns>The index of the last item less than the specified value, or -1 if no such item exists.</returns>
        public int IndexOfLastLessThan(T value)
        {
            Contract.Ensures(Contract.Result<int>() >= -1 && Contract.Result<int>() < Count);

            Node leaf;
            int index;
            Node.Find(_root, value, AllowsDuplicates ? -1 : 0, out leaf, out index);
            return Node.GetRootIndex(leaf, index) - 1;
        }

        /// <summary>
        /// Get all items equal to or greater than the specified value, starting with the lowest index and moving forwards. O(log N), move next: O(1)
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>All items having values equal to or greater than the specified value.</returns>
        public IEnumerable<T> EnumerateFrom(T value)
        {
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            Node leaf;
            int index;
            Node.Find(_root, value, AllowsDuplicates ? -1 : 0, out leaf, out index);
            return Node.EnumerateFromIndex(leaf, index);
        }

        /// <summary>
        /// Get all items less than or equal to the specified value, starting with the highest index and moving backwards. O(log N), move next: O(1)
        /// </summary>
        /// <param name="value">The key value.</param>
        /// <returns>All items having values equal to or greater than the specified value.</returns>
        public IEnumerable<T> EnumerateBackwardsFrom(T value)
        {
            Contract.Ensures(Contract.Result<IEnumerable<T>>() != null);

            Node leaf;
            int index;
            var found = Node.Find(_root, value, AllowsDuplicates ? 1 : 0, out leaf, out index);
            if (!found)
                --index;
            return Node.EnumerateBackwardsFromIndex(leaf, index);
        }
        
        #endregion
    }
}
